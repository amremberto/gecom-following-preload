using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.CreateDocumentType;

/// <summary>
/// Validator for CreateDocumentTypeCommand.
/// </summary>
internal sealed class CreateDocumentTypeCommandValidator : AbstractValidator<CreateDocumentTypeCommand>
{
    public CreateDocumentTypeCommandValidator()
    {
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

