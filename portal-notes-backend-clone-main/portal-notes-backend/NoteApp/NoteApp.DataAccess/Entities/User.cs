using System.Collections.Generic;

namespace NoteApp.DataAccess.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public ICollection<Folder>? Folders { get; set; }
        public ICollection<NoteShare>? SharedNotes { get; set; }  // Kullanıcıyla paylaşılan notlar
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ProfileImageUrl { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
} 