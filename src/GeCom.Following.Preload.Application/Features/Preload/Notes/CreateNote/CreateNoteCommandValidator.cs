using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.CreateNote;

/// <summary>
/// Validator for CreateNoteCommand.
/// </summary>
internal sealed class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.");

        RuleFor(x => x.UsuarioCreacion)
            .NotEmpty()
            .WithMessage("UsuarioCreacion is required.");
    }
}

