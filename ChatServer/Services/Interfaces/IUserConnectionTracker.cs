namespace ChatServer.Services.Interfaces
{
    public interface IUserConnectionTracker
    {
        Task AddConnectionAsync(Guid userId, string connectionId);
        Task RemoveConnectionAsync(Guid userId, string connectionId);
        Task<int> GetConnectionCountAsync(Guid userId);
        Task<IReadOnlyList<string>> GetConnectionsAsync(Guid userId);
        Task<IReadOnlyList<string>> GetConnectionsAsync(IReadOnlyList<Guid> userIds);
    }
}
