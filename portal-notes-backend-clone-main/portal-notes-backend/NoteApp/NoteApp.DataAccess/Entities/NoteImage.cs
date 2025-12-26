using System.ComponentModel.DataAnnotations;

namespace NoteApp.DataAccess.Entities
{
    public class NoteImage
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string? ImageUrl { get; set; }
        public Note? Note { get; set; }
    }
} 