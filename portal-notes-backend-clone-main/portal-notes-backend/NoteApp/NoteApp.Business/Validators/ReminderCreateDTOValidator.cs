using FluentValidation;
using NoteApp.Business.DTOs;

namespace NoteApp.Business.Validators
{
    public class ReminderCreateDTOValidator : AbstractValidator<ReminderCreateDTO>
    {
        public ReminderCreateDTOValidator()
        {
            RuleFor(x => x.NoteId)
                .NotEmpty().WithMessage("Not ID boş bırakılamaz")
                .GreaterThan(0).WithMessage("Geçerli bir not ID giriniz");

            RuleFor(x => x.ReminderDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Hatırlatma tarihi gelecekte olmalıdır");

            RuleFor(x => x.Message)
                .MaximumLength(500).WithMessage("Hatırlatma mesajı en fazla 500 karakter olabilir");
        }
    }
}
