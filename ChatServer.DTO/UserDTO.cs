namespace ChatServer.DTO
{
    public enum EnumStatus
    {
        Online,
        Offline
    }
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public EnumStatus Status { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
