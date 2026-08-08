using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ChatServer.Services
{
    public class AuthService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordService passwordService,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IAuthService
    {
        public async Task<AuthResult> LoginAsync(string username, string password, string ipAddress)
        {
            var user = await users.GetByUsernameAsync(username);
            if (user is null) return new AuthResult.InvalidCredentials();

            if (user.LockoutEndAt is { } until && until > DateTime.UtcNow)
                return new AuthResult.AccountLocked(until);

            if (!passwordService.VerifyPassword(user, password))
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                    user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
                await users.SaveChangesAsync();
                return new AuthResult.InvalidCredentials();
            }

            user.AccessFailedCount = 0;
            user.LockoutEndAt = null;

            var access = tokenService.GenerateAccessToken(user);
            var (rawRefresh, refreshHash) = tokenService.GenerateRefreshToken();

            refreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            await users.SaveChangesAsync();
            await refreshTokens.SaveChangesAsync();

            return new AuthResult.Success(access, rawRefresh);
        }

        public async Task<AuthResult> RefreshAsync(string refreshToken, string ipAddress)
        {
            var hash = tokenService.HashRefreshToken(refreshToken);
            var stored = await refreshTokens.GetByHashAsync(hash);

            if (stored is null) return new AuthResult.InvalidCredentials();

            if (!stored.IsActive)
            {
                await refreshTokens.RevokeAllActiveForUserAsync(stored.UserId);
                return new AuthResult.SessionCompromised();
            }

            stored.RevokedAt = DateTime.UtcNow;
            var (newRaw, newHash) = tokenService.GenerateRefreshToken();
            var newToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = stored.UserId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
            stored.ReplacedByTokenId = newToken.Id;
            refreshTokens.Add(newToken);
            await refreshTokens.SaveChangesAsync();

            var access = tokenService.GenerateAccessToken(stored.User);
            return new AuthResult.Success(access, newRaw);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var hash = tokenService.HashRefreshToken(refreshToken);
            var stored = await refreshTokens.GetByHashAsync(hash);
            if (stored is not null)
            {
                stored.RevokedAt = DateTime.UtcNow;
                await refreshTokens.SaveChangesAsync();
            }
        }
    }
}
