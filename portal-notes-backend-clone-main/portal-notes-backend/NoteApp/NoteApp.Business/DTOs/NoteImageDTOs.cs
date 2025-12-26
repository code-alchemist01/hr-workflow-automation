namespace NoteApp.Business.DTOs
{
    public class NoteImageCreateDTO
    {
        public int NoteId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class NoteImageUpdateDTO
    {
        public string? ImageUrl { get; set; }
    }

    public class NoteImageResponseDTO
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string? ImageUrl { get; set; }
    }
} 