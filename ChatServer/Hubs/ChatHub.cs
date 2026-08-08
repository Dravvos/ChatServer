using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Hubs
{
    public class ChatHub:Hub
    {
        Task SendMessage(Guid conversationId, string content);
        Task JoinConversation(Guid conversationId);
        Task Typing(Guid conversationId);
        Task MarkAsRead(Guid conversationId, Guid messageId);

        public override Task OnConnectedAsync();    // marca usuário online, notifica contatos
        public override Task OnDisconnectedAsync(); // marca offline
    }
}
