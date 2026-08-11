using ChatServer.Common.Enums;

namespace ChatServer.DTO
{
    public record UserProfileDto(Guid Id, string Username, string Email, string? AvatarUrl, UserStatus Status);
}
