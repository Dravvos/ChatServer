using ChatServer.Common.Enums;

namespace ChatServer.DTO
{
    public record UserSummaryDto(Guid Id, string Username, string? AvatarUrl, UserStatus Status);
}
