using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class NoteCreateDTOValidator : AbstractValidator<NoteCreateDTO>
    {
        public NoteCreateDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Not başlığı boş bırakılamaz")
                .MinimumLength(2).WithMessage("Not başlığı en az 2 karakter olmalıdır")
                .MaximumLength(100).WithMessage("Not başlığı en fazla 100 karakter olabilir");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Not içeriği boş bırakılamaz")
                .MaximumLength(10000).WithMessage("Not içeriği en fazla 10000 karakter olabilir");

            // FolderId artık opsiyonel, zorunlu değil

            RuleForEach(x => x.TagIds)
                .GreaterThan(0).WithMessage("Geçerli bir tag ID giriniz")
                .When(x => x.TagIds != null);
        }
    }
} 