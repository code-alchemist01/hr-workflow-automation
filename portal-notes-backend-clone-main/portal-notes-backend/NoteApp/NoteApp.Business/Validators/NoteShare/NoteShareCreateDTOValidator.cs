using FluentValidation;
using NoteApp.Business.DTOs.NoteShare;

namespace NoteApp.Business.Validators.NoteShare
{
    public class NoteShareCreateDTOValidator : AbstractValidator<NoteShareCreateDTO>
    {
        public NoteShareCreateDTOValidator()
        {
            RuleFor(x => x.NoteId)
                .NotEmpty().WithMessage("NoteId is required");

            RuleFor(x => x.SharedWithUserId)
                .NotEmpty().WithMessage("SharedWithUserId is required");

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(System.DateTime.UtcNow)
                .When(x => x.ExpiresAt.HasValue)
                .WithMessage("ExpiresAt must be in the future");
        }
    }
} 