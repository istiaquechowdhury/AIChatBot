using AIChatBot.Web.Hubs;
using AIChatBot.Web.Interfaces;
using AIChatBot.Web.Models;
using AIChatBot.Web.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AIChatBot.Web.APIControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TavilyService _tavilyService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IUnitOfWork unitOfWork, TavilyService tavilyService, IHubContext<ChatHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _tavilyService = tavilyService;
            _hubContext = hubContext;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return BadRequest("Message is required.");

            var userMessage = new ChatMessage
            {
                UserId = GetUserId(),
                Message = messageText,
                Sender = "user",
                Timestamp = DateTime.UtcNow,
                IsApproved = true,
                IsDeleted = false
            };

            await _unitOfWork.ChatMessages.AddMessageAsync(userMessage);
            await _unitOfWork.CompleteAsync();

            // TODO: Later call Tavily AI and SignalR here

            var botReplyText = await _tavilyService.GetAnswerAsync(messageText);
            Console.WriteLine($"Tavily AI replied: {botReplyText}");

            var botMessage = new ChatMessage
            {
                UserId = GetUserId(),
                Message = botReplyText,
                Sender = "bot",
                Timestamp = DateTime.UtcNow,
                IsApproved = true,
                IsDeleted = false
            };

            await _unitOfWork.ChatMessages.AddMessageAsync(botMessage);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "user", userMessage.Message);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "bot", botMessage.Message);


            return Ok(new { userMessage, botMessage });
        }



        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory(int page = 1, int pageSize = 10)
        {
            var messages = await _unitOfWork.ChatMessages
                .GetMessagesByUserAsync(GetUserId(), page, pageSize);

            return Ok(messages);
        }
    }
}
