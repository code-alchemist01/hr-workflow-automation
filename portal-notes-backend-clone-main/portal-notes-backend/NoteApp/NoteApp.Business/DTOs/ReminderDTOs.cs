using System;

namespace NoteApp.Business.DTOs
{
    public class ReminderCreateDTO
    {
        public int NoteId { get; set; }               // Hangi nota bağlı
        public DateTime ReminderDate { get; set; }    // Hatırlatma zamanı
        public string Message { get; set; } = string.Empty;
    }

    public class ReminderUpdateDTO
    {
        public DateTime ReminderDate { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ReminderResponseDTO
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public DateTime ReminderDate { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
