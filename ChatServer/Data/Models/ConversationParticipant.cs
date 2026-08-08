namespace ChatServer.Data.Models
{
    public class ConversationParticipant
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = default!;
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public ParticipantRole Role { get; set; } = ParticipantRole.Member;
        public DateTime JoinedAt { get; set; }
        public DateTime? LastReadAt { get; set; }
    }

    public enum ParticipantRole { Member, Admin }
}
