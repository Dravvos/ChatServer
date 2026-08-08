using ChatServer.Data.Models;

namespace ChatServer.Data.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashAsync(string hash);
        void Add(RefreshToken refreshToken);
        Task RevokeAllActiveForUserAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
