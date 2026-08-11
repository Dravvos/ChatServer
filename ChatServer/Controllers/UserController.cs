using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()=>
           await userService.GetProfileAsync(CurrentUserId) is { } profile ? Ok(profile) : NotFound();

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q) =>
       Ok(await userService.SearchAsync(CurrentUserId, q));

        [HttpPut("me/avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] string avatarUrl) =>
            Ok(await userService.UpdateAvatarAsync(CurrentUserId, avatarUrl));
    }
}
