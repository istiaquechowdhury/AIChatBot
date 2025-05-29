using System.ComponentModel.DataAnnotations;

namespace AIChatBot.Web.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string? SessionId { get; set; }

        [Required]
        public string Sender { get; set; }  

        [Required]
        public string Message { get; set; }

        public bool IsApproved { get; set; } = false; 

        public bool IsDeleted { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
