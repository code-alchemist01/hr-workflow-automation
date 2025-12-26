using System.ComponentModel.DataAnnotations;

namespace NoteApp.DataAccess.Entities
{
    public class NoteAudio
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string? AudioUrl { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Note? Note { get; set; }
    }
} 