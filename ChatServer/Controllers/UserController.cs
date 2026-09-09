using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        private Guid CurrentUserId => Guid.Parse(User.Claims.FirstOrDefault()?.Value!);

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            var userId = await userService.GetUserIdByUsername(username);
            return Ok(userId);
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser() =>
           await userService.GetProfileAsync(CurrentUserId) is { } profile ? Ok(profile) : NotFound();

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q) =>
       Ok(await userService.SearchAsync(CurrentUserId, q));

        [HttpPut("me/avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] string avatarUrl) =>
            Ok(await userService.UpdateAvatarAsync(CurrentUserId, avatarUrl));
    }
}
