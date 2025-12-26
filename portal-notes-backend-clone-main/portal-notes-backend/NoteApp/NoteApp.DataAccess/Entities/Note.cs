using System.Collections.Generic;

namespace NoteApp.DataAccess.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public int? FolderId { get; set; }
        public bool IsArchived { get; set; } = false; // Varsay�lan olarak hi�bir not ar�ivli de�il.
        public Folder? Folder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<NoteImage>? NoteImages { get; set; }
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
        public ICollection<NoteAudio> NoteAudios { get; set; } = new List<NoteAudio>();
        public ICollection<Reminder>? Reminders { get; set; }
      
        public ICollection<NoteShare>? Shares { get; set; }
    }
} 