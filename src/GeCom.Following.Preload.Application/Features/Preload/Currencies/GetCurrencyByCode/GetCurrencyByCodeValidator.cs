using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyByCode;

/// <summary>
/// Validator for the GetCurrencyByCodeQuery.
/// </summary>
internal sealed class GetCurrencyByCodeValidator : AbstractValidator<GetCurrencyByCodeQuery>
{
    public GetCurrencyByCodeValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Currency code is required.");
    }
}

