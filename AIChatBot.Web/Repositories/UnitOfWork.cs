using AIChatBot.Web.Data;
using AIChatBot.Web.Interfaces;

namespace AIChatBot.Web.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IChatMessageRepository ChatMessages { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ChatMessages = new ChatMessageRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
