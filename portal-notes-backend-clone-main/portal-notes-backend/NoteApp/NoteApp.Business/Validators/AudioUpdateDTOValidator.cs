using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class AudioUpdateDTOValidator : AbstractValidator<AudioUpdateDTO>
    {
        private readonly string[] _allowedExtensions = { ".mp3", ".wav", ".ogg", ".m4a" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public AudioUpdateDTOValidator()
        {
            RuleFor(x => x.AudioFile)
                .NotNull().WithMessage("Ses dosyası gerekli")
                .Must(file => file != null && file.Length > 0).WithMessage("Ses dosyası boş olamaz")
                .Must(file => file != null && file.Length <= MaxFileSize)
                    .WithMessage($"Dosya boyutu {MaxFileSize / (1024 * 1024)}MB'dan büyük olamaz")
                .Must(file => file != null && _allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                    .WithMessage($"Dosya tipi şunlardan biri olmalıdır: {string.Join(", ", _allowedExtensions)}");
        }
    }
} 