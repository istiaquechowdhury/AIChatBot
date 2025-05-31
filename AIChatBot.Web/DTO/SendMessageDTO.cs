using System.ComponentModel.DataAnnotations;

namespace AIChatBot.Web.DTO
{
    public class SendMessageDTO
    {
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Message { get; set; }
    }
}
