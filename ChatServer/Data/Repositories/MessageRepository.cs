using ChatServer.Common.Enums;
using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class MessageRepository(ChatDbContext db) : IMessageRepository
    {
        public void Add(Message message) => db.Messages.Add(message);

        public Task<int> CountUnreadAsync(Guid conversationId, Guid userId, DateTime? since)=>
                        db.Messages
                .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && m.IsDeleted == false && m.SentAt > (since ?? DateTime.MinValue));


        public Task<IReadOnlyList<Message>> GetPageAsync(Guid conversationId, DateTime? before, int take) =>
            db.Messages.AsNoTracking()
                .Where(m => m.ConversationId == conversationId && m.IsDeleted == false && (before == null || m.SentAt < before))
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .Include(m => m.Sender)
                .ToListAsync()
                .ContinueWith(t => (IReadOnlyList<Message>)t.Result);

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message is not null)
            {
                message.Status = MessageStatus.Read;
                await db.SaveChangesAsync();
            }
        }

        public Task SaveChangesAsync() => db.SaveChangesAsync();
    }
}
