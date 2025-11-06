using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.UpdateSociety;

/// <summary>
/// Validator for UpdateSocietyCommand.
/// </summary>
internal sealed class UpdateSocietyCommandValidator : AbstractValidator<UpdateSocietyCommand>
{
    public UpdateSocietyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than zero.");

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

