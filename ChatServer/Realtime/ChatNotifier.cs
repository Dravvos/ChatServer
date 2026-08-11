using ChatServer.Common.Enums;
using ChatServer.Contracts;
using ChatServer.Hubs;
using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Realtime
{
    public class ChatNotifier(IHubContext<ChatHub> hub, IUserConnectionTracker connections) : IChatNotifier
    {
        public async Task NotifyMessageReadAsync(IReadOnlyList<Guid> recipientUserIds, Guid conversationId, Guid readerUserId, Guid messageId)
        {
            var connectionIds = await connections.GetConnectionsAsync(recipientUserIds);
            await hub.Clients.Clients(connectionIds).SendAsync("MessageRead", conversationId, readerUserId, messageId);
        }

        public async Task NotifyMessageReceivedAsync(IReadOnlyList<Guid> recipientUserIds, MessageDto message)
        {
            var connectionIds = await connections.GetConnectionsAsync(recipientUserIds);
            await hub.Clients.Clients(connectionIds).SendAsync("MessageReceived", message);

        }

        public async Task NotifyTypingAsync(IReadOnlyList<Guid> recipientUserIds, Guid conversationId, Guid userId)
        {
            var connectionIds = await connections.GetConnectionsAsync(recipientUserIds);
            await hub.Clients.Clients(connectionIds).SendAsync("Typing", conversationId, userId);

        }

        public async Task NotifyUserStatusChangedAsync(Guid userId, UserStatus status)
        {
            await hub.Clients.All.SendAsync("UserStatusChanged", userId, status);
        }
    }
}
