using ChatServer.Data.Models;

namespace ChatServer.Services.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(User user, string password);
        bool VerifyPassword(User user, string password);
    }
}
