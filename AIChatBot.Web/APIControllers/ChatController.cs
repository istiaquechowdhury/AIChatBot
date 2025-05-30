using AIChatBot.Web.Data;
using AIChatBot.Web.DTO;
using AIChatBot.Web.Hubs;
using AIChatBot.Web.Interfaces;
using AIChatBot.Web.Models;
using AIChatBot.Web.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Security.Claims;

namespace AIChatBot.Web.APIControllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TavilyService _tavilyService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public ChatController(IUnitOfWork unitOfWork, TavilyService tavilyService, IHubContext<ChatHub> hubContext, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _tavilyService = tavilyService;
            _hubContext = hubContext;
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);


        private bool IsAdmin() => User.IsInRole("Admin");



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
                IsApproved = false,
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
                IsApproved = false,
                IsDeleted = false
            };

            await _unitOfWork.ChatMessages.AddMessageAsync(botMessage);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "user", userMessage.Message);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "bot", botMessage.Message);


            return Ok(new { userMessage, botMessage });
        }


        [HttpPut("{id}")]
        
        public async Task<IActionResult> EditMessage(int id, [FromBody] EditMessageDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message cannot be empty.");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var message = await _unitOfWork.ChatMessages.GetMessageByIdAsync(id);
            if (message == null || message.IsDeleted)
                return NotFound("Message not found or already deleted.");

            // Only the sender or admin can edit
            //bool isAdmin = User.IsInRole("Admin");
            //if (message.UserId != userId && !isAdmin)
            //    return Forbid();

            message.Message = dto.Message;

            await _unitOfWork.CompleteAsync();

            return Ok("Message updated.");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetMessageByIdAsync(id);

            //if (message.Sender == "bot")
            //    return BadRequest("Cannot delete bot message");

            if (message == null || message.IsDeleted) return BadRequest();

            message.IsDeleted = true;
            await _unitOfWork.CompleteAsync();
            return Ok("message softDelete successfull");



        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> ApproveMessage(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetMessageByIdAsync(id);

            if (message == null || message.IsDeleted)
                return NotFound("Message not found or already deleted.");

            if (message.IsApproved)
                return BadRequest("Message already approved.");

            message.IsApproved = true;
            await _unitOfWork.CompleteAsync();

            return Ok("Message approved successfully.");
        }

        [HttpGet("history")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetApprovedMessages(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Securely get logged-in user's ID

            var messages = await _context.ChatMessages
                .Where(m => m.IsApproved && !m.IsDeleted && m.UserId == userId)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages); // Final order from oldest to newest
        }

        //[HttpGet("history")]
        //public async Task<IActionResult> GetChatHistory(int page = 1, int pageSize = 10)
        //{
        //    var messages = await _unitOfWork.ChatMessages
        //        .GetMessagesByUserAsync(GetUserId(), page, pageSize);

        //    return Ok(messages);
        //}

        //[HttpGet("admin/all-messages")]
        //public async Task<IActionResult> GetAllMessagesForAdmin()
        //{
        //    if (!IsAdmin())
        //        return Forbid();

        //    var allMessages = await _context.ChatMessages
        //        .Where(m => !m.IsDeleted)
        //        .OrderBy(m => m.Timestamp)
        //        .ToListAsync();

        //    return Ok(allMessages);
        //}
        [HttpGet("admin/all-messages")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMessagesForAdmin(int page = 1, int pageSize = 10)
        {
            var messages = await _context.ChatMessages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
