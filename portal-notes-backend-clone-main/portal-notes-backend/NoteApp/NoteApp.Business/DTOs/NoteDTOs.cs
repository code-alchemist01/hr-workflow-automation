using System.Collections.Generic;
using NoteApp.DataAccess.Entities;
using NoteApp.Business.DTOs.Tag;

namespace NoteApp.Business.DTOs
{
    public class NoteCreateDTO
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public int? FolderId { get; set; }
        public List<int>? TagIds { get; set; }
    }

    public class NoteUpdateDTO
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public int? FolderId { get; set; } // <-- Burayý nullable yaptýk
        public List<int>? TagIds { get; set; }
    }

    public class NoteResponseDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public int? FolderId { get; set; } // <-- Burayý nullable yaptýk
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TagResponseDTO>? Tags { get; set; }
        public List<AudioResponseDTO>? NoteAudios { get; set; }
    }
}