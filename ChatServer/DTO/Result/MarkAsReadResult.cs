namespace ChatServer.DTO.Result
{
    public abstract record MarkAsReadResult
    {
        public sealed record Success : MarkAsReadResult;
        public sealed record NotParticipant : MarkAsReadResult;
    }
}
