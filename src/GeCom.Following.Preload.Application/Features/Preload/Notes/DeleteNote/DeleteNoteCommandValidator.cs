using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.DeleteNote;

/// <summary>
/// Validator for DeleteNoteCommand.
/// </summary>
internal sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.NotaId)
            .GreaterThan(0)
            .WithMessage("NotaId must be greater than 0.");
    }
}

