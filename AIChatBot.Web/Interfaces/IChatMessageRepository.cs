using AIChatBot.Web.Models;

namespace AIChatBot.Web.Interfaces
{
    public interface IChatMessageRepository
    {
        Task<IEnumerable<ChatMessage>> GetMessagesByUserAsync(string userId, int page, int pageSize);
        Task AddMessageAsync(ChatMessage message);
        Task<ChatMessage?> GetMessageByIdAsync(int id);
        Task UpdateMessageAsync(ChatMessage message);
    }
}
