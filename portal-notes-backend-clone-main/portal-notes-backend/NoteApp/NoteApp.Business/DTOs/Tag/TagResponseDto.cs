namespace NoteApp.Business.DTOs.Tag
{
    public class TagResponseDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
} 