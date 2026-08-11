using ChatServer.DTO.Request;
using ChatServer.DTO.Result;
using ChatServer.Services;
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
    public class ConversationController(ConversationService conversationService) : ControllerBase
    {
        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        [HttpGet]
        public async Task<IActionResult> GetMyConversations()
        {
            var conversations = await conversationService.GetUserConversationsAsync(CurrentUserId);
            return Ok(conversations);
        }

        [HttpPost("direct")]
        public async Task<IActionResult> CreateDirectConversation([FromBody] Guid otherUserId) =>
            (await conversationService.CreateDirectAsync(CurrentUserId, otherUserId)) switch
        {
            ConversationResult.Success s => Ok(s.conversation),
            ConversationResult.InvalidOperation inv => BadRequest(inv.reason),
            _ => BadRequest()
        };

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupConversationRequest request) =>
            (await conversationService.CreateGroupAsync(CurrentUserId, request.Name, request.ParticipantIds)) switch
        {
            ConversationResult.Success s => Ok(s.conversation),
            ConversationResult.InvalidOperation inv => BadRequest(inv.reason),
            _ => BadRequest()
        };

        [HttpGet("{conversationId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId, [FromQuery] DateTime? before, [FromQuery] int pageSize = 30) =>
        (await conversationService.GetMessagesAsync(CurrentUserId, conversationId, before, Math.Clamp(pageSize, 1, 100))) switch
        {
            MessagesQueryResult.Success s => Ok(new { messages = s.messages, hasMore = s.hasMore }),
            _ => Forbid()
        };

        [HttpPost("{conversationId:guid}/participants")]
        public async Task<IActionResult> AddParticipant(Guid conversationId, Guid userId) =>
            (await conversationService.AddParticipantAsync(CurrentUserId, conversationId, userId)) switch
            {
                ConversationResult.Success s => Ok(s.conversation),
                ConversationResult.Forbidden => Forbid(),
                ConversationResult.InvalidOperation inv => BadRequest(inv.reason),
                _ => NotFound()
            };

        [HttpDelete("{conversationId:guid}/participants/{userId:guid}")]
        public async Task<IActionResult> RemoveParticipant(Guid conversationId, Guid userId) =>
            (await conversationService.RemoveParticipantAsync(CurrentUserId, conversationId, userId)) switch
            {
                ConversationResult.Success => NoContent(),
                ConversationResult.Forbidden => Forbid(),
                ConversationResult.InvalidOperation inv => BadRequest(inv.reason),
                _ => NotFound()
            };
    }
}
