using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.CreateSociety;

/// <summary>
/// Validator for CreateSocietyCommand.
/// </summary>
internal sealed class CreateSocietyCommandValidator : AbstractValidator<CreateSocietyCommand>
{
    public CreateSocietyCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Codigo is required.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.");

        RuleFor(x => x.Cuit)
            .NotEmpty()
            .WithMessage("CUIT is required.");
    }
}

