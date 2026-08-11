using ChatServer.DTO;
using ChatServer.DTO.Result;

namespace ChatServer.Services.Interfaces
{
    public interface IConversationService
    {
        Task<IReadOnlyList<ConversationSummaryDto>> GetUserConversationsAsync(Guid userId);
        Task<ConversationResult> CreateDirectAsync(Guid requesterId, Guid otherUserId);
        Task<ConversationResult> CreateGroupAsync(Guid requesterId, string name, IReadOnlyList<Guid> participantIds);
        Task<MessagesQueryResult> GetMessagesAsync(Guid requesterId, Guid conversationId, DateTime? before, int pageSize);
        Task<ConversationResult> AddParticipantAsync(Guid requesterId, Guid conversationId, Guid newUserId);
        Task<ConversationResult> RemoveParticipantAsync(Guid requesterId, Guid conversationId, Guid targetUserId);
    }
}
