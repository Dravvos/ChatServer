using ChatServer.Common.Enums;
using ChatServer.Data.Repositories.Interfaces;
using ChatServer.Services.Interfaces;
using System.Collections.Concurrent;

namespace ChatServer.Services
{
    public class PresenceService(IUserConnectionTracker connections, IChatNotifier notifier,
        IServiceScopeFactory scopeFactory) : IPresenceService
    {
        private static readonly TimeSpan OfflineGracePeriod = TimeSpan.FromSeconds(5);
        // um CTS pendente por usuário = "vou anunciar Offline, a menos que alguém cancele"
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _pendingOffline = new();

        public async Task UserConnectedAsync(Guid userId, string connectionId)
        {
            await connections.AddConnectionAsync(userId, connectionId);

            // havia um "Offline" agendado (reconexão rápida)? cancela — nunca chegou a acontecer de verdade
            if (_pendingOffline.TryRemove(userId, out var pendingCts))
            {
                pendingCts.Cancel();
                pendingCts.Dispose();
                return; // do ponto de vista de quem observa, o usuário nunca saiu do Online
            }

            if (await connections.GetConnectionCountAsync(userId) == 1)
            {
                await PersistStatusAsync(userId, UserStatus.Online);
                await notifier.NotifyUserStatusChangedAsync(userId, UserStatus.Online);
            }
        }

        public async Task UserDisconnectedAsync(Guid userId, string connectionId)
        {
            await connections.RemoveConnectionAsync(userId, connectionId);
            if (await connections.GetConnectionCountAsync(userId) != 0)
                return; // ainda tem outra conexão ativa (outra aba/dispositivo) — continua online

            var cts = new CancellationTokenSource();
            if (!_pendingOffline.TryAdd(userId, cts))
            {
                cts.Dispose();
                return; // já existe uma checagem pendente pra esse usuário
            }

            try
            {
                await Task.Delay(OfflineGracePeriod, cts.Token);

                // sobreviveu à janela sem ser cancelado e continua com 0 conexões: offline de verdade
                if (await connections.GetConnectionCountAsync(userId) == 0)
                {
                    await PersistStatusAsync(userId, UserStatus.Offline);
                    await notifier.NotifyUserStatusChangedAsync(userId, UserStatus.Offline); 
                }
            }
            catch (TaskCanceledException)
            {
                // reconectou dentro da janela de tolerância — nunca chega a anunciar Offline
            }
            finally
            {
                _pendingOffline.TryRemove(userId, out _);
                cts.Dispose();
            }
        }
        private async Task PersistStatusAsync(Guid userId, UserStatus status)
        {
            using var scope = scopeFactory.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = await users.GetByIdAsync(userId);
            if (user is null || user.Status == status) return; // nada mudou, evita SaveChanges à toa

            user.Status = status;
            await users.SaveChangesAsync();
        }
    }
}
