using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NoteApp.Business.Services;
using NoteApp.WebApi.Hubs;

namespace NoteApp.WebApi.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NoteHub> _hubContext;

        public SignalRNotificationService(IHubContext<NoteHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNoteUpdateAsync(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNoteUpdate", message);
        }

        public async Task NotifyUserAsync(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveUserNotification", message);
        }

        public async Task NotifyNoteGroupAsync(string noteId, string message)
        {
            await _hubContext.Clients.Group($"Note_{noteId}").SendAsync("ReceiveNoteUpdate", message);
        }
    }
} 