namespace ChatServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string username, string password, string ipAddress);
        Task<AuthResult> RefreshAsync(string refreshToken, string ipAddress);
        Task LogoutAsync(string refreshToken);
    }
    public abstract record AuthResult
    {
        public sealed record Success(string AccessToken, string RefreshToken) : AuthResult;
        public sealed record InvalidCredentials : AuthResult;
        public sealed record AccountLocked(DateTime Until) : AuthResult;
        public sealed record SessionCompromised : AuthResult; // reuse de refresh token detectado
    }
}
