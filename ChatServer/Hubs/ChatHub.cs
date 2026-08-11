using ChatServer.Common.Enums;
using ChatServer.DTO.Result;
using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Hubs
{
    [Authorize]
    public class ChatHub(IChatService chatService, IUserConnectionTracker connections, IChatNotifier notifier) : Hub
    {
        private Guid UserId => Guid.Parse(Context.UserIdentifier!);

        public override async Task OnConnectedAsync()
        {
            await connections.AddConnectionAsync(UserId, Context.ConnectionId);
            if (await connections.GetConnectionCountAsync(UserId) == 1)
                await notifier.NotifyUserStatusChangedAsync(UserId, UserStatus.Online);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await connections.RemoveConnectionAsync(UserId, Context.ConnectionId);
            if (await connections.GetConnectionCountAsync(UserId) == 0)
                await notifier.NotifyUserStatusChangedAsync(UserId, UserStatus.Offline);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Guid conversationId, string content)
        {
            var result = await chatService.SendMessageAsync(UserId, conversationId, content);
            if (result is SendMessageResult.NotParticipant)
                throw new HubException("Você não participa dessa conversa.");
            if (result is SendMessageResult.InvalidContent invalid)
                throw new HubException(invalid.Reason);
            // Success: notificação já foi disparada dentro do ChatService
        }

        public Task MarkAsRead(Guid conversationId, Guid messageId) =>
            chatService.MarkAsReadAsync(UserId, conversationId, messageId);

        public Task Typing(Guid conversationId) =>
            chatService.NotifyTypingAsync(UserId, conversationId);
    }
}
