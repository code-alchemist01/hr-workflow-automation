using NoteApp.Business.DTOs.Tag;

namespace NoteApp.Business.DTOs
{
    public class FolderCreateDTO
    {
        public string? Name { get; set; }
        public string? Color { get; set; }
        public int UserId { get; set; }
        public List<int>? TagIds { get; set; }
    }

    public class FolderUpdateDTO
    {
        public string? Name { get; set; }
        public string? Color { get; set; }
        public List<int>? TagIds { get; set; }
    }

    public class FolderResponseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public int UserId { get; set; }
        public List<TagResponseDTO>? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 