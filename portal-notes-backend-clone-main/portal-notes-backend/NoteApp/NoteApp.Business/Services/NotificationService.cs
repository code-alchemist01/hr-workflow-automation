using System.Threading.Tasks;

namespace NoteApp.Business.Services
{
    public interface INotificationService
    {
        Task NotifyNoteUpdateAsync(string message);
        Task NotifyUserAsync(string userId, string message);
        Task NotifyNoteGroupAsync(string noteId, string message);
    }

    public class NotificationService : INotificationService
    {
        public async Task NotifyNoteUpdateAsync(string message)
        {
            // Bu metod WebApi katmanında implement edilecek
            await Task.CompletedTask;
        }

        public async Task NotifyUserAsync(string userId, string message)
        {
            // Bu metod WebApi katmanında implement edilecek
            await Task.CompletedTask;
        }

        public async Task NotifyNoteGroupAsync(string noteId, string message)
        {
            // Bu metod WebApi katmanında implement edilecek
            await Task.CompletedTask;
        }
    }
} 