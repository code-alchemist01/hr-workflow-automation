using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NoteApp.Business.DTOs
{
    public class AudioCreateDTO
    {
        [Required(ErrorMessage = "NoteID gerekli")]
        public int NoteId { get; set; }

        [Required(ErrorMessage = "Ses dosyası gerekli")]
        public IFormFile? AudioFile { get; set; }
    }

    public class AudioUpdateDTO
    {
        [Required(ErrorMessage = "Ses dosyası gerekli")]
        public IFormFile? AudioFile { get; set; }
    }

    public class AudioResponseDTO
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string? AudioUrl { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 