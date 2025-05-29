using Microsoft.AspNetCore.SignalR;
namespace AIChatBot.Web.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendTypingNotification(string user)
        {
            await Clients.All.SendAsync("ReceiveTypingNotification");
        }

        public async Task StopTypingNotification(string user)
        {
            await Clients.All.SendAsync("StopTypingNotification");
        }
    }
}
