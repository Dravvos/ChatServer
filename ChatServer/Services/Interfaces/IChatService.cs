using ChatServer.DTO.Result;

namespace ChatServer.Services.Interfaces
{
    public interface IChatService
    {
        Task<SendMessageResult> SendMessageAsync(Guid senderId, Guid conversationId, string content);
        Task<MarkAsReadResult> MarkAsReadAsync(Guid userId, Guid conversationId, Guid messageId);
        Task NotifyTypingAsync(Guid userId, Guid conversationId);
    }
}
