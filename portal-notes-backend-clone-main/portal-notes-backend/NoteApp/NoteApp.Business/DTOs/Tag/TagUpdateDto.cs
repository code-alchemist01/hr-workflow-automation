using System.ComponentModel.DataAnnotations;

namespace NoteApp.Business.DTOs.Tag
{
    public class TagUpdateDto
    {
        [Required(ErrorMessage = "Tag ID zorunludur")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tag başlığı zorunludur")]
        [MinLength(2, ErrorMessage = "Tag başlığı en az 2 karakter olmalıdır")]
        [MaxLength(50, ErrorMessage = "Tag başlığı en fazla 50 karakter olabilir")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Tag rengi zorunludur")]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Geçerli bir hex renk kodu giriniz (örn: #FF0000)")]
        public string? Color { get; set; }
    }
} 