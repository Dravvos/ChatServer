using ChatServer.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data
{
    public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>(e =>
            {
                e.HasIndex(u => u.Username).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Username).HasMaxLength(32);
                e.Property(u => u.PasswordHash).IsRequired();
            });

            b.Entity<ConversationParticipant>(e =>
            {
                e.HasKey(cp => new { cp.ConversationId, cp.UserId });
                e.HasOne(cp => cp.Conversation)
                 .WithMany(c => c.Participants)
                 .HasForeignKey(cp => cp.ConversationId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(cp => cp.User)
                 .WithMany(u => u.Participations)
                 .HasForeignKey(cp => cp.UserId)
                 .OnDelete(DeleteBehavior.Restrict); // não apagar usuário em cascata por engano
            });

            b.Entity<Message>(e =>
            {
                e.Property(m => m.Content).HasMaxLength(4000);
                e.HasIndex(m => new { m.ConversationId, m.SentAt }); // paginação de histórico
                e.HasOne(m => m.Sender)
                 .WithMany()
                 .HasForeignKey(m => m.SenderId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<RefreshToken>(e =>
            {
                e.HasIndex(r => r.TokenHash).IsUnique();
                e.HasOne(r => r.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
