using System.Collections.Generic;

namespace NoteApp.DataAccess.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
        public ICollection<FolderTag> FolderTags { get; set; } = new List<FolderTag>();
    }
} 