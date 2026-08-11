using ChatServer.Common.Enums;
using ChatServer.Contracts;

namespace ChatServer.Services.Interfaces
{
    public interface IChatNotifier
    {
        Task NotifyMessageReceivedAsync(IReadOnlyList<Guid> recipientUserIds, MessageDto message);
        Task NotifyMessageReadAsync(IReadOnlyList<Guid> recipientUserIds, Guid conversationId, Guid readerUserId, Guid messageId);
        Task NotifyTypingAsync(IReadOnlyList<Guid> recipientUserIds, Guid conversationId, Guid userId);
        Task NotifyUserStatusChangedAsync(Guid userId, UserStatus status);
    }
}
