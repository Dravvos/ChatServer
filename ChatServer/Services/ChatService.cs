 using ChatServer.Common.Enums;
using ChatServer.Contracts;
using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.DTO;
using ChatServer.DTO.Result;
using ChatServer.Services.Interfaces;

namespace ChatServer.Services
{
    public class ChatService(IConversationRepository conversations,
        IMessageRepository messages,
        IChatNotifier notifier) : IChatService
    {
        public Task DeleteMessageAsync(Guid userId, Guid conversationId, Guid messageId)
        {
            throw new NotImplementedException();
        }

        public async Task<MarkAsReadResult> MarkAsReadAsync(Guid userId, Guid conversationId, Guid messageId)
        {
            var conversation = await conversations.GetByIdWithParticipantsAsync(conversationId);
            var participant = conversation?.Participants.SingleOrDefault(p => p.UserId == userId);
            if (participant is null) return new MarkAsReadResult.NotParticipant();

            participant.LastReadAt = DateTime.UtcNow;
            await conversations.SaveChangesAsync();

            await messages.MarkAsReadAsync(messageId);

            var recipientIds = conversation!.Participants.Select(p => p.UserId).Where(id => id != userId).ToList();
            await notifier.NotifyMessageReadAsync(recipientIds, conversationId, userId, messageId);

            return new MarkAsReadResult.Success();
        }

        public async Task NotifyTypingAsync(Guid userId, Guid conversationId)
        {
            var conversation = await conversations.GetByIdWithParticipantsAsync(conversationId);
            if (conversation is null || conversation.Participants.All(p => p.UserId != userId)) return;

            var recipientIds = conversation.Participants.Select(p => p.UserId).Where(id => id != userId).ToList();
            await notifier.NotifyTypingAsync(recipientIds, conversationId, userId);
        }

        public async Task<SendMessageResult> SendMessageAsync(Guid senderId, Guid conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new SendMessageResult.InvalidContent("Mensagem vazia.");

            if (content.Length > 4000)
                return new SendMessageResult.InvalidContent("Mensagem excede 4000 caracteres.");

            var conversation = await conversations.GetByIdWithParticipantsAsync(conversationId);
            if (conversation is null || conversation.Participants.All(p => p.UserId != senderId))
                return new SendMessageResult.NotParticipant();

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
                Status = MessageStatus.Sent
            };
            messages.Add(message);
            await messages.SaveChangesAsync();

            var dto = new MessageDto(message.Id, conversationId, senderId,
                conversation.Participants.First(p => p.UserId == senderId).User.Username,
                message.Content, message.SentAt, null, message.Status);

            var recipientIds = conversation.Participants.Select(p => p.UserId).ToList();
            await notifier.NotifyMessageReceivedAsync(recipientIds, dto);

            return new SendMessageResult.Success(dto);
        }
    }
}
