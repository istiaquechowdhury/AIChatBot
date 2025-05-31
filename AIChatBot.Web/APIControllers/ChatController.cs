using AIChatBot.Web.Data;
using AIChatBot.Web.DTO;
using AIChatBot.Web.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIChatBot.Web.APIControllers
{
    [ApiController]
    [Route("api/[controller]")]
    
   
    
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController( ApplicationDbContext context, IChatService chatService)
        {
            _chatService = chatService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);



        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO dto)
        {
            

            if(!ModelState.IsValid)
            return BadRequest(ModelState);  

            var userId = GetUserId(); 

            var result = await _chatService.SendMessageAsync(dto, userId);

            return Ok(new { result.userMessage, result.botMessage });
        }


        
        [HttpPut("{id}")]
        public async Task<IActionResult> EditMessage(int id, [FromBody] EditMessageDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var result = await _chatService.EditMessageAsync(id, dto.Message, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            await _chatService.DeleteMessageAsync(id); 


            return Ok("Message SoftDeleted Successfully");
        }


        
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveMessage(int id)
        {
           await _chatService.ApproveMessage(id);
          

            return Ok("Message approved successfully.");
        }



        [HttpGet("history")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetChatHistory(int page = 1, int pageSize = 10)
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messages = await _chatService.GetChatHistory(UserId, page, pageSize);

            return Ok(messages);
        }



        [HttpGet("admin/all-messages")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMessagesForAdmin(int page = 1, int pageSize = 10)
        {
            var messages = await _chatService.GetMessages(page, pageSize);
            return Ok(messages);
        }


    }
}
