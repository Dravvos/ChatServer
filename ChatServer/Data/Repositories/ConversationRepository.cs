using ChatServer.Common.Enums;
using ChatServer.Data.Models;
using ChatServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data.Repositories
{
    public class ConversationRepository(ChatDbContext db) : IConversationRepository
    {
        public void Add(Conversation conversation)=>
            db.Conversations.Add(conversation);

        public Task<bool> DirectConversationExistsAsync(Guid userAId, Guid userBId)=>
            db.Conversations
                .Include(c => c.Participants)
                .AnyAsync(c => c.Type == ConversationType.Direct &&
                               c.Participants.Any(p => p.UserId == userAId) &&
                               c.Participants.Any(p => p.UserId == userBId));

        public Task<Conversation?> GetByIdWithParticipantsAsync(Guid id) =>
            db.Conversations.Include(c => c.Participants).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);

        public Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId) =>
            db.Conversations.AsNoTracking()
            .Where(c=>c.Participants.Any(p => p.UserId == userId))
                .Include(c => c.Participants)
                .Include(p => p.Messages.OrderByDescending(m=>m.SentAt).Take(1))
                .ToListAsync()
            .ContinueWith(t=> (IReadOnlyList<Conversation>)t.Result);

        public Task SaveChangesAsync() => db.SaveChangesAsync();
    }
}
