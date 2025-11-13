using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.DeleteCurrency;

/// <summary>
/// Validator for the DeleteCurrencyCommand.
/// </summary>
internal sealed class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
{
    public DeleteCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Currency ID must be greater than 0.");
    }
}

