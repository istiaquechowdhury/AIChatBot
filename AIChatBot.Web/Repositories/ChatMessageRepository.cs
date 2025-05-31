using AIChatBot.Web.Data;
using AIChatBot.Web.Interfaces;
using AIChatBot.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AIChatBot.Web.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByUserAsync(string userId, int page, int pageSize)
        {
                return await _context.ChatMessages
                .Where(m => m.IsApproved && !m.IsDeleted && m.UserId == userId)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int id)
        {
            return await _context.ChatMessages.FindAsync(id);
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
        }

        

        public async Task<IEnumerable<ChatMessage>> GetAllUndeletedMessages(int page, int pageSize)
        {
                 return await _context.ChatMessages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
        }
    }
}
