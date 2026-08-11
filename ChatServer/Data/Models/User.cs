using ChatServer.Common.Enums;

namespace ChatServer.Data.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;   // hash, nunca a senha
        public string SecurityStamp { get; set; } = default!;  // invalida tokens ao trocar senha
        public string? AvatarUrl { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Offline;
        public DateTime CreatedAt { get; set; }

        public int AccessFailedCount { get; set; }             // brute force
        public DateTime? LockoutEndAt { get; set; }

        public ICollection<ConversationParticipant> Participations { get; set; } = [];
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }

}
