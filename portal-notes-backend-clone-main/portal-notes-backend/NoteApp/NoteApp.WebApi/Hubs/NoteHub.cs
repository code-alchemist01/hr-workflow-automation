using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NoteApp.WebApi.Hubs
{
    [Authorize]
    public class NoteHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendUserNotification(string userId, string message)
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveUserNotification", message);
        }

        public async Task JoinNoteGroup(string noteId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Note_{noteId}");
        }

        public async Task LeaveNoteGroup(string noteId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Note_{noteId}");
        }
    }
} 