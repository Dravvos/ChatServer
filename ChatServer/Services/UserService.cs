using ChatServer.Contracts;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.Services.Interfaces;

namespace ChatServer.Services
{
    public class UserService(IUserRepository users) : IUserService
    {
        public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
        {
            var user = await users.GetByIdAsync(userId);
            if (user == null)
                return null;
            return new(

                user.Id,
                user.Username,
                user.Email,
                user.AvatarUrl,
                user.Status
            );
        }

        public Task<Guid> GetUserIdByUsername(string username) =>
            users.GetUserIdByUsername(username);

        public async Task<IReadOnlyList<UserSummaryDto>> SearchAsync(Guid requesterId, string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2) return [];
            var found = await users.SearchAsync(query, requesterId, limit: 20);
            return found.Select(u => new UserSummaryDto(u.Id, u.Username, u.AvatarUrl, u.Status)).ToList();

        }

        public async Task<UserProfileDto> UpdateAvatarAsync(Guid userId, string avatarUrl)
        {
            var user = await users.GetByIdAsync(userId)
                  ?? throw new InvalidOperationException("Usuário autenticado não encontrado — inconsistência de dados.");
            user.AvatarUrl = avatarUrl;
            await users.SaveChangesAsync();
            return new(user.Id, user.Username, user.Email, user.AvatarUrl, user.Status);

        }
    }
}
