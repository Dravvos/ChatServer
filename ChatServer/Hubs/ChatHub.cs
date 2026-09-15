using ChatServer.Common.Enums;
using ChatServer.DTO.Result;
using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Hubs
{
    [Authorize]
    public class ChatHub(IChatService chatService, IUserConnectionTracker connections, IChatNotifier notifier,
        IPresenceService presence) : Hub
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

            await presence.UserConnectedAsync(userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (UserId is not Guid userId)
            {
                // Logue o erro e derrube a conexão. O cliente receberá um erro claro.
                Context.Abort();
                return;
            }
            await presence.UserDisconnectedAsync(userId, Context.ConnectionId);
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
