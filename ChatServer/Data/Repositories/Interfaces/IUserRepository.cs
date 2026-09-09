using ChatServer.Data.Models;

namespace ChatServer.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task SaveChangesAsync();
        Task<IReadOnlyList<User>> SearchAsync(string query, Guid requesterId, int limit = 10);
        void Add(User user); // e em UserRepository: db.Users.Add(user);
        public Task<Guid> GetUserIdByUsername(string username);
    }
}
