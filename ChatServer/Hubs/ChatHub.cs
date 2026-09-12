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
        private Guid? UserId
        {
            get
            {
                if (Guid.TryParse(Context.User.Claims.FirstOrDefault()?.Value, out var userId))
                    return userId;
                return null;
            }
        }

        public override async Task OnConnectedAsync()
        {
            if (UserId is not Guid userId)
            {
                // Logue o erro e derrube a conexão. O cliente receberá um erro claro.
                Context.Abort();
                return;
            }

            await connections.AddConnectionAsync(userId, Context.ConnectionId);
            if (await connections.GetConnectionCountAsync(userId) == 1)
                await notifier.NotifyUserStatusChangedAsync(userId, UserStatus.Online);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await connections.RemoveConnectionAsync(UserId.GetValueOrDefault(), Context.ConnectionId);
            if (await connections.GetConnectionCountAsync(UserId.GetValueOrDefault()) == 0)
                await notifier.NotifyUserStatusChangedAsync(UserId.GetValueOrDefault(), UserStatus.Offline);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Guid conversationId, string content)
        {
            var result = await chatService.SendMessageAsync(UserId.GetValueOrDefault(), conversationId, content);
            if (result is SendMessageResult.NotParticipant)
                throw new HubException("Você não participa dessa conversa.");
            if (result is SendMessageResult.InvalidContent invalid)
                throw new HubException(invalid.Reason);
            // Success: notificação já foi disparada dentro do ChatService
        }

        public Task MarkAsRead(Guid conversationId, Guid messageId) =>
            chatService.MarkAsReadAsync(UserId.GetValueOrDefault(), conversationId, messageId);

        public Task Typing(Guid conversationId) =>
            chatService.NotifyTypingAsync(UserId.GetValueOrDefault(), conversationId);
    }
}
