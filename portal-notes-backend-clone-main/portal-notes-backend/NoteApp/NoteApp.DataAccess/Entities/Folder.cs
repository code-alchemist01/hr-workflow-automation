using System.Collections.Generic;

namespace NoteApp.DataAccess.Entities
{
    public class Folder
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Note>? Notes { get; set; }
        public ICollection<FolderTag> FolderTags { get; set; } = new List<FolderTag>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 