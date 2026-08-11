namespace ChatServer.DTO.Request
{
    public class CreateGroupConversationRequest
    {
        public List<Guid> ParticipantIds { get; set; } = new List<Guid>();
        public string Name { get; set; } = "";
    }
}
