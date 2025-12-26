using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class ReminderUpdateDTOValidator : AbstractValidator<ReminderUpdateDTO>
    {
        public ReminderUpdateDTOValidator()
        {
            RuleFor(x => x.ReminderDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Hatırlatma tarihi gelecekte olmalıdır");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Hatırlatma mesajı boş bırakılamaz")
                .MaximumLength(500).WithMessage("Hatırlatma mesajı en fazla 500 karakter olabilir");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("Aktiflik durumu belirtilmelidir");
        }
    }
}
