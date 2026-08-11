using ChatServer.Common.Enums;

namespace ChatServer.Contracts
{
    public record UserProfileDto(Guid Id, string Username, string Email, string? AvatarUrl, UserStatus Status);
}
