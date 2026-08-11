using ChatServer.Common.Enums;

namespace ChatServer.Contracts
{
    public record UserSummaryDto(Guid Id, string Username, string? AvatarUrl, UserStatus Status);
}
