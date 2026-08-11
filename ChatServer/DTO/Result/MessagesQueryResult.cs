namespace ChatServer.DTO.Result
{
    public abstract record MessagesQueryResult
    {
        public sealed record Success(IReadOnlyList<MessageDto> messages, bool hasMore) : MessagesQueryResult;
        public sealed record Forbidden : MessagesQueryResult;
    }
}
