using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class RefreshTokenRepository(ChatDbContext db) : IRefreshTokenRepository
    {
        public void Add(RefreshToken refreshToken) => db.RefreshTokens.Add(refreshToken);

        public Task<RefreshToken?> GetByHashAsync(string hash) =>
            db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.TokenHash == hash);

        public Task RevokeAllActiveForUserAsync(Guid userId) =>
            db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null)
                .ForEachAsync(x => x.RevokedAt = DateTime.UtcNow);

        public Task SaveChangesAsync() => db.SaveChangesAsync();
    }
}
