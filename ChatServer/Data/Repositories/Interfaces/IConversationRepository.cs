using ChatServer.Data.Models;

namespace ChatServer.Data.Repositories.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdWithParticipantsAsync(Guid id);
        Task<bool> DirectConversationExistsAsync(Guid userAId, Guid userBId);
        Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId); // com última mensagem via filtered include
        void Add(Conversation conversation);
        Task SaveChangesAsync();
    }
}
