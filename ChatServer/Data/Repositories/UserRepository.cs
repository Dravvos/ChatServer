using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class UserRepository(ChatDbContext db) : IUserRepository
    {
        public Task<User?> GetByUsernameAsync(string username)=>
            db.Users.FirstOrDefaultAsync(u => u.Username == username);

        public Task SaveChangesAsync()=> db.SaveChangesAsync();
    }
}
