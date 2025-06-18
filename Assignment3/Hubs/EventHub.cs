using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Assignment3.Hubs
{
    [Authorize]
    public class EventHub : Hub
    {
        public async Task JoinEventGroup(string eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Event_{eventId}");
        }

        public async Task LeaveEventGroup(string eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Event_{eventId}");
        }

        public async Task SendEventUpdate(string eventId, string message)
        {
            await Clients.Group($"Event_{eventId}").SendAsync("EventUpdated", message);
        }

        public async Task SendAttendeeUpdate(string eventId, string message)
        {
            await Clients.Group($"Event_{eventId}").SendAsync("AttendeeUpdated", message);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", "You are connected to the event hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
} 