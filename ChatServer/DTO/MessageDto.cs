using ChatServer.Common.Enums;

namespace ChatServer.DTO
{
    public record MessageDto(Guid id, Guid conversationId, Guid senderId, string senderUsername, string content, DateTime sentAt,
        DateTime? editedAt, MessageStatus status);
}
