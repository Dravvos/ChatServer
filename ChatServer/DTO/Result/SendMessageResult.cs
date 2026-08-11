using ChatServer.Contracts;

namespace ChatServer.DTO.Result
{
    public abstract record SendMessageResult
    {
        public sealed record Success(MessageDto Message) : SendMessageResult;
        public sealed record NotParticipant : SendMessageResult;
        public sealed record InvalidContent(string Reason) : SendMessageResult;
    }
}
