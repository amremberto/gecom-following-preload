using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.UpdateCurrency;

/// <summary>
/// Validator for the UpdateCurrencyCommand.
/// </summary>
internal sealed class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.MonedaId)
            .GreaterThan(0)
            .WithMessage("Currency ID must be greater than 0.");

        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Currency code is required.")
            .MaximumLength(4)
            .WithMessage("Currency code must not exceed 4 characters.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Currency description is required.");

        RuleFor(x => x.CodigoAfip)
            .MaximumLength(3)
            .WithMessage("AFIP code must not exceed 3 characters.")
            .When(x => x.CodigoAfip is not null);
    }
}

