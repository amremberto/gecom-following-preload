using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.UpdateNote;

/// <summary>
/// Validator for UpdateNoteCommand.
/// </summary>
internal sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.NotaId)
            .GreaterThan(0)
            .WithMessage("NotaId must be greater than 0.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.");
    }
}

