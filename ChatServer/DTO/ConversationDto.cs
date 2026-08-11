using ChatServer.Common.Enums;

namespace ChatServer.DTO
{
    public record ConversationDto(Guid id, ConversationType Type, string? name, IReadOnlyList<Guid> participantIds );
}
