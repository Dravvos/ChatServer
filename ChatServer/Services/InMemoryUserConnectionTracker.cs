using ChatServer.Services.Interfaces;
using System.Collections.Concurrent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatServer.Services
{
    public class InMemoryUserConnectionTracker : IUserConnectionTracker
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _map = new();
        public Task AddConnectionAsync(Guid userId, string connectionId)
        {
            _map.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>())[connectionId] = 0;
            return Task.CompletedTask;
        }

        public Task<int> GetConnectionCountAsync(Guid userId)=>
            Task.FromResult(_map.TryGetValue(userId, out var set) ? set.Count : 0);

        public Task<IReadOnlyList<string>> GetConnectionsAsync(Guid userId)
            => Task.FromResult<IReadOnlyList<string>>(_map.TryGetValue(userId, out var set) ? set.Keys.ToList() : []);

        public Task<IReadOnlyList<string>> GetConnectionsAsync(IReadOnlyList<Guid> userIds)
            => Task.FromResult<IReadOnlyList<string>>(
            userIds.Where(_map.ContainsKey).SelectMany(id => _map[id].Keys).ToList());

        public Task RemoveConnectionAsync(Guid userId, string connectionId)
        {
            if (_map.TryGetValue(userId, out var set))
            {
                set.TryRemove(connectionId, out _);
                if (set.IsEmpty) _map.TryRemove(userId, out _);
            }
            return Task.CompletedTask;
        }
    }
}
