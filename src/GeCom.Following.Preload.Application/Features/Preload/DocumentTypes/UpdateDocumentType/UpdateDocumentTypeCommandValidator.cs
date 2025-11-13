using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.UpdateDocumentType;

/// <summary>
/// Validator for UpdateDocumentTypeCommand.
/// </summary>
internal sealed class UpdateDocumentTypeCommandValidator : AbstractValidator<UpdateDocumentTypeCommand>
{
    public UpdateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.TipoDocId)
            .GreaterThan(0)
            .WithMessage("TipoDocId must be greater than 0.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.")
            .MaximumLength(90)
            .WithMessage("Descripcion must not exceed 90 characters.");

        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Codigo is required.")
            .MaximumLength(4)
            .WithMessage("Codigo must not exceed 4 characters.");

        RuleFor(x => x.Letra)
            .MaximumLength(1)
            .WithMessage("Letra must not exceed 1 character.")
            .When(x => x.Letra is not null);

        RuleFor(x => x.DescripcionLarga)
            .MaximumLength(90)
            .WithMessage("DescripcionLarga must not exceed 90 characters.")
            .When(x => x.DescripcionLarga is not null);
    }
}

