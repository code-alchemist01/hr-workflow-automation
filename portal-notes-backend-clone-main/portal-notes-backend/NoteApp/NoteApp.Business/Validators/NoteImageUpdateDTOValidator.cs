using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class NoteImageUpdateDTOValidator : AbstractValidator<NoteImageUpdateDTO>
    {
        public NoteImageUpdateDTOValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Resim URL'si boş bırakılamaz")
                .MaximumLength(500).WithMessage("Resim URL'si en fazla 500 karakter olabilir");
        }
    }
} 