using ChatServer.Common.Enums;

namespace ChatServer.Data.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = default!;
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }                      // soft delete
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }
}
