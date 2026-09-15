using ChatServer.Common.Enums;

namespace ChatServer.Contracts
{
    public record ConversationSummaryDto(Guid id, ConversationType type, string? name, string? lastMessagePreview, DateTime? lastMessageAt, int unreadCount, Guid? otherParticipantId,
        string? otherParticipantName, UserStatus? otherParticipantStatus);
}
