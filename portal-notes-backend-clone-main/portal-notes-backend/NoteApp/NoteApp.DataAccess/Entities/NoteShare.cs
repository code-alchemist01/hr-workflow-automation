using System;

namespace NoteApp.DataAccess.Entities
{
    public class NoteShare
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public Note? Note { get; set; }
        public int SharedWithUserId { get; set; }
        public User? SharedWithUser { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime SharedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 