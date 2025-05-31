using AIChatBot.Web.DTO;
using AIChatBot.Web.Hubs;
using AIChatBot.Web.Interfaces;
using AIChatBot.Web.Models;
using Microsoft.AspNetCore.SignalR;

namespace AIChatBot.Web.Service
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TavilyService _tavilyService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(IUnitOfWork unitOfWork, TavilyService tavilyService, IHubContext<ChatHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _tavilyService = tavilyService;
            _hubContext = hubContext;
        }

        public async Task<(ChatMessage userMessage, ChatMessage botMessage)> SendMessageAsync(SendMessageDTO dto, string userId)
        {
            var userMessage = new ChatMessage
            {
                UserId = userId,
                Message = dto.Message,
                Sender = "user",
                Timestamp = DateTime.UtcNow,
                IsApproved = false,
                IsDeleted = false
            };

            await _unitOfWork.ChatMessages.AddMessageAsync(userMessage);
            await _unitOfWork.CompleteAsync();

            var botReplyText = await _tavilyService.GetAnswerAsync(dto.Message);

            var botMessage = new ChatMessage
            {
                UserId = userId,
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

            return (userMessage, botMessage);
        }


        public async Task<string> EditMessageAsync(int id, string newMessage, string userId)
        {
            if (string.IsNullOrWhiteSpace(newMessage))
                throw new ArgumentException("Message cannot be empty.");

            var message = await _unitOfWork.ChatMessages.GetMessageByIdAsync(id);
            if (message == null || message.IsDeleted)
                throw new KeyNotFoundException("Message not found or already deleted.");

            

            message.Message = newMessage;

            await _unitOfWork.CompleteAsync();

            return "Message updated.";
        }

        public async Task DeleteMessageAsync(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetMessageByIdAsync(id);

            if (message == null || message.IsDeleted)
                throw new KeyNotFoundException("Message not found or already deleted.");

            message.IsDeleted = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task ApproveMessage(int id)
        {
            var message = await _unitOfWork.ChatMessages.GetMessageByIdAsync(id);

            if (message == null || message.IsDeleted)
            throw new KeyNotFoundException("Message not found or already deleted.");

            if (message.IsApproved)
            throw new KeyNotFoundException("Message already approved.");

            message.IsApproved = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetChatHistory(string userId, int page, int pageSize)
        {
            var messages = await _unitOfWork.ChatMessages
                .GetMessagesByUserAsync(userId, page, pageSize);

            return messages;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessages(int page, int pageSize)
        {
            return await _unitOfWork.ChatMessages.GetAllUndeletedMessages(page, pageSize);
            
        }
    }
}
