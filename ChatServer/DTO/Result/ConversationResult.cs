namespace ChatServer.DTO.Result
{
    public abstract record ConversationResult
    {
        public sealed record Success(ConversationDto conversation) : ConversationResult;
        public sealed record NotFound : ConversationResult;
        public sealed record Forbidden : ConversationResult;
        public sealed record InvalidOperation(string reason) : ConversationResult;
    }
}
