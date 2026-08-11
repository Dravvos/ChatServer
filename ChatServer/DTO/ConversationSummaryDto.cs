using ChatServer.Common.Enums;

namespace ChatServer.DTO
{
    public record ConversationSummaryDto(Guid id, ConversationType type, string? name, string? lastMessagePreview, DateTime? lastMessageAt, int unreadCount);
}
