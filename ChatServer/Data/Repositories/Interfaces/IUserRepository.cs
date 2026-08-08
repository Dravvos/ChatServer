using ChatServer.Data.Models;

namespace ChatServer.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task SaveChangesAsync();
    }
}
