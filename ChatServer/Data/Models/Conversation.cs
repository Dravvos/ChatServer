using ChatServer.Common.Enums;

namespace ChatServer.Data.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }              // Direct | Group
        public string? Name { get; set; }                       // só para Group
        public DateTime CreatedAt { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; } = [];
        public ICollection<Message> Messages { get; set; } = [];
    }
}
