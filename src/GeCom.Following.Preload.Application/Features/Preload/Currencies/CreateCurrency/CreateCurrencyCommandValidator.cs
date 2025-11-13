using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.CreateCurrency;

/// <summary>
/// Validator for the CreateCurrencyCommand.
/// </summary>
internal sealed class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
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

