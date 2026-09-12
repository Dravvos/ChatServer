using ChatServer.Data.Models;

namespace ChatServer.Data.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        void Add(Message message);
        Task<IReadOnlyList<Message>> GetPageAsync(Guid conversationId, DateTime? before, int take);
        Task<int> CountUnreadAsync(Guid conversationId, Guid userId, DateTime? since);
        Task SaveChangesAsync();
        Task MarkAsReadAsync(Guid messageId);
    }
}
