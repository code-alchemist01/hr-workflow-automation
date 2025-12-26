

namespace NoteApp.DataAccess.Entities
{
    public class Reminder
    {
        public int Id { get; set; }  // Primary Key
        public int NoteId { get; set; }      // Foreign Key
        public Note? Note { get; set; }      // Navigation Property

        public DateTime ReminderDate { get; set; }  // Hatırlatma zamanı
        public string Message { get; set; } = string.Empty; // Hatırlatma mesajı
        public bool IsActive { get; set; } = true;  // Aktif mi kontrolü
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
