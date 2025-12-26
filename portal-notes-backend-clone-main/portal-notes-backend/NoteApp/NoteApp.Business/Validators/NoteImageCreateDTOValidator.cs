using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class NoteImageCreateDTOValidator : AbstractValidator<NoteImageCreateDTO>
    {
        public NoteImageCreateDTOValidator()
        {
            RuleFor(x => x.NoteId)
                .NotEmpty().WithMessage("Not ID boş bırakılamaz")
                .GreaterThan(0).WithMessage("Geçerli bir not ID giriniz");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Resim URL'si boş bırakılamaz")
                .MaximumLength(500).WithMessage("Resim URL'si en fazla 500 karakter olabilir");
        }
    }
} 