using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class FolderCreateDTOValidator : AbstractValidator<FolderCreateDTO>
    {
        public FolderCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Klasör adı boş bırakılamaz")
                .MinimumLength(2).WithMessage("Klasör adı en az 2 karakter olmalıdır")
                .MaximumLength(50).WithMessage("Klasör adı en fazla 50 karakter olabilir");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID boş bırakılamaz")
                .GreaterThan(0).WithMessage("Geçerli bir kullanıcı ID giriniz");
        }
    }
} 