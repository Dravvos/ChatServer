namespace ChatServer.Data.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string TokenHash { get; set; } = default!;        // NUNCA guardar o token puro
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = default!;
        public DateTime? RevokedAt { get; set; }
        public Guid? ReplacedByTokenId { get; set; }              // rastro de rotação

        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    }
}
