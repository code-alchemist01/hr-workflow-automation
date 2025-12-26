using FluentValidation;
using NoteApp.Business.DTOs.NoteShare;

namespace NoteApp.Business.Validators.NoteShare
{
    public class NoteShareUpdateDTOValidator : AbstractValidator<NoteShareUpdateDTO>
    {
        public NoteShareUpdateDTOValidator()
        {
            RuleFor(x => x.ExpiresAt)
                .Must((dto, expiresAt) => !expiresAt.HasValue || expiresAt.Value > DateTime.UtcNow)
                .WithMessage("Expiration date must be in the future");
        }
    }
} 