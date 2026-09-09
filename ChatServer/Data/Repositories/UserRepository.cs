using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class UserRepository(ChatDbContext db) : IUserRepository
    {
        public void Add(User user) =>
            db.Users.Add(user);

        public Task<User?> GetByIdAsync(Guid id) =>
                        db.Users.SingleOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByUsernameAsync(string username) =>
            db.Users.FirstOrDefaultAsync(u => u.Username == username);

        public Task SaveChangesAsync() => db.SaveChangesAsync();

        public Task<IReadOnlyList<User>> SearchAsync(string query, Guid requesterId, int limit = 10) =>
           db.Users.AsNoTracking().Where(u => u.Id != requesterId && EF.Functions.ILike(u.Username, $"%{query}%")) // Postgres
            .OrderBy(u => u.Username).Take(limit).ToListAsync()
            .ContinueWith(t => (IReadOnlyList<User>)t.Result);

        public Task<Guid> GetUserIdByUsername(string username)=>
            db.Users.Where(x=>x.Username == username).Select(x=>x.Id).FirstOrDefaultAsync();
    }
}
