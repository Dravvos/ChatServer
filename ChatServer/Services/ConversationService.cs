using ChatServer.Common.Enums;
using ChatServer.Contracts;
using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.DTO;
using ChatServer.DTO.Result;
using ChatServer.Services.Interfaces;

namespace ChatServer.Services
{
    public class ConversationService(IConversationRepository conversations,
        IMessageRepository messages,
        IUserRepository users) : IConversationService
    {

        public async Task<IReadOnlyList<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId)
        {
            var userConversations = await conversations.GetForUserAsync(userId);
            var result = new List<ConversationSummaryDto>();

            foreach (var c in userConversations)
            {
                var participant = c.Participants.First(p => p.UserId == userId);
                var lastMessage = c.Messages.FirstOrDefault(); // já vem limitado a 1 (filtered include)
                var unread = await messages.CountUnreadAsync(c.Id, userId, participant.LastReadAt);

                result.Add(new ConversationSummaryDto(
                    c.Id, c.Type, c.Name, lastMessage?.Content, lastMessage?.SentAt, unread));
            }

            return result;
            // Nota: N+1 aceitável para dezenas de conversas por usuário. Se crescer muito,
            // dá pra trocar por uma query única projetada com GroupBy.
        }

        public async Task<ConversationResult> CreateDirectAsync(Guid requesterId, Guid otherUserId)
        {
            if (requesterId == otherUserId)
                return new ConversationResult.InvalidOperation("Não é possível criar uma conversa consigo mesmo.");

            if (await users.GetByIdAsync(otherUserId) is null)
                return new ConversationResult.InvalidOperation("Usuário não encontrado.");

            if (await conversations.DirectConversationExistsAsync(requesterId, otherUserId))
                return new ConversationResult.InvalidOperation("Já existe uma conversa direta com esse usuário.");

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Direct,
                CreatedAt = DateTime.UtcNow,
                Participants =
                [
                    new ConversationParticipant { UserId = requesterId, JoinedAt = DateTime.UtcNow },
                new ConversationParticipant { UserId = otherUserId, JoinedAt = DateTime.UtcNow }
                ]
            };

            conversations.Add(conversation);
            await conversations.SaveChangesAsync();
            return new ConversationResult.Success(ToDto(conversation));
        }

        public async Task<ConversationResult> CreateGroupAsync(Guid requesterId, string name, IReadOnlyList<Guid> participantIds)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                return new ConversationResult.InvalidOperation("Nome do grupo inválido.");

            var distinctIds = participantIds.Append(requesterId).Distinct().ToList();
            if (distinctIds.Count < 3)
                return new ConversationResult.InvalidOperation("Um grupo precisa de pelo menos 3 participantes.");

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Group,
                Name = name.Trim(),
                CreatedAt = DateTime.UtcNow,
                Participants = distinctIds.Select(id => new ConversationParticipant
                {
                    UserId = id,
                    JoinedAt = DateTime.UtcNow,
                    Role = id == requesterId ? ParticipantRole.Admin : ParticipantRole.Member
                }).ToList()
            };

            conversations.Add(conversation);
            await conversations.SaveChangesAsync();
            return new ConversationResult.Success(ToDto(conversation));
        }

        public async Task<MessagesQueryResult> GetMessagesAsync(Guid requesterId, Guid conversationId, DateTime? before, int pageSize)
        {
            var conversation = await conversations.GetByIdWithParticipantsAsync(conversationId);
            // NotFound e Forbidden retornam o mesmo resultado pro cliente: evita confirmar
            // se a conversa existe pra quem não participa dela.
            if (conversation is null || conversation.Participants.All(p => p.UserId != requesterId))
                return new MessagesQueryResult.Forbidden();

            var page = await messages.GetPageAsync(conversationId, before, pageSize + 1);
            var hasMore = page.Count > pageSize;
            var items = page.Take(pageSize)
                .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderId, m.Sender.Username,
                                             m.Content, m.SentAt, m.EditedAt, m.Status))
                .ToList();

            return new MessagesQueryResult.Success(items, hasMore);
        }

        public async Task<ConversationResult> AddParticipantAsync(Guid requesterId, Guid conversationId, Guid newUserId)
        {
            var conversation = await conversations.GetByIdWithParticipantsAsync(conversationId);
            if (conversation is null) return new ConversationResult.NotFound();
            if (conversation.Type != ConversationType.Group)
                return new ConversationResult.InvalidOperation("Só é possível adicionar participantes em grupos.");

            var requester = conversation.Participants.SingleOrDefault(p => p.UserId == requesterId);
            if (requester is null || requester.Role != ParticipantRole.Admin)
                return new ConversationResult.Forbidden();

            if (conversation.Participants.Any(p => p.UserId == newUserId))
                return new ConversationResult.InvalidOperation("Usuário já participa do grupo.");

            conversation.Participants.Add(new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = newUserId,
                JoinedAt = DateTime.UtcNow,
                Role = ParticipantRole.Member
            });

            await conversations.SaveChangesAsync();
            return new ConversationResult.Success(ToDto(conversation));
        }

        public async Task<ConversationResult> RemoveParticipantAsync(Guid requesterId, Guid conversationId, Guid targetUserId)
        {
            var conversation = await conversations.GetByIdWithParticipantsAsync(conversationId);
            if (conversation is null) return new ConversationResult.NotFound();

            var requester = conversation.Participants.SingleOrDefault(p => p.UserId == requesterId);
            var isSelfRemoval = requesterId == targetUserId;
            if (requester is null || (requester.Role != ParticipantRole.Admin && !isSelfRemoval))
                return new ConversationResult.Forbidden();

            var target = conversation.Participants.SingleOrDefault(p => p.UserId == targetUserId);
            if (target is null) return new ConversationResult.InvalidOperation("Usuário não participa do grupo.");

            conversation.Participants.Remove(target); // chave é composta → EF trata como delete, não como FK nula
            await conversations.SaveChangesAsync();
            return new ConversationResult.Success(ToDto(conversation));
        }

        private static ConversationDto ToDto(Conversation c) =>
            new(c.Id, c.Type, c.Name, c.Participants.Select(p => p.UserId).ToList());
    }
}
