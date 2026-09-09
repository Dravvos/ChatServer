using ChatServer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController(IChatService chatService) : ControllerBase
    {
        private Guid CurrentUserId => Guid.Parse(User.Claims.FirstOrDefault()?.Value!);

    }
}
