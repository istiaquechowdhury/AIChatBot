using AIChatBot.Web.DTO;
using AIChatBot.Web.Models;

namespace AIChatBot.Web.Service
{
    public interface IChatService
    {
        Task<(ChatMessage userMessage, ChatMessage botMessage)> SendMessageAsync(SendMessageDTO dto, string userId);
        Task<string> EditMessageAsync(int id, string newMessage, string userId);
        Task DeleteMessageAsync(int id);
        Task ApproveMessage(int id);
        Task<IEnumerable<ChatMessage>> GetChatHistory(string userId, int page, int pageSize);
        Task<IEnumerable<ChatMessage>> GetMessages(int page, int pageSize);
    }
}
