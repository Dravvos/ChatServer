using ChatServer.Contracts;

namespace ChatServer.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetProfileAsync(Guid userId);
        Task<IReadOnlyList<UserSummaryDto>> SearchAsync(Guid requesterId, string query);
        Task<UserProfileDto> UpdateAvatarAsync(Guid userId, string avatarUrl);
    }
}
