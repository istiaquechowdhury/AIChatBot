namespace AIChatBot.Web.Interfaces
{
    public interface IUnitOfWork
    {
        IChatMessageRepository ChatMessages { get; }
        Task<int> CompleteAsync();
    }
}
