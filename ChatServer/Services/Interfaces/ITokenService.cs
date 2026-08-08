using ChatServer.Data.Models;

namespace ChatServer.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        (string rawToken, string hash) GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}
