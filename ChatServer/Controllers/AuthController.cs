using ChatServer.DTO.Request;
using ChatServer.DTO.Response;
using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(ChatServer.DTO.Request.LoginRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await authService.LoginAsync(req.Username, req.Password, ip);

            return result switch
            {
                AuthResult.Success success => Ok(new AuthResponse(success.AccessToken, success.RefreshToken)),
                AuthResult.InvalidCredentials => Unauthorized(new { message = "Invalid credentials" }),
                AuthResult.AccountLocked locked => StatusCode(StatusCodes.Status423Locked, new { until = locked.Until }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unknown error" })
            };
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Refresh(RefreshRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await authService.RefreshAsync(req.RefreshToken, ip);
            return result switch
            {
                AuthResult.Success success => Ok(new { accessToken = success.AccessToken, refreshToken = success.RefreshToken }),
                AuthResult.SessionCompromised => Unauthorized(new { message = "Session compromised. Login again" }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unknown error" })
            };
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(RefreshRequest req)
        {
            await authService.LogoutAsync(req.RefreshToken);
            return NoContent();
        }

        [HttpPost("signup")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> SignUp(SignUpRequest req)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await authService.SignUpAsync(req.Username, req.Email, req.Password, ip);
            return result switch
            {
                AuthResult.Success success => Ok(new AuthResponse(success.AccessToken, success.RefreshToken)),
                AuthResult.ValidationFailed invalid => BadRequest(new { message = invalid.Reason }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unknown error" })
            };
        }
    }
}
