using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyById;

/// <summary>
/// Validator for the GetCurrencyByIdQuery.
/// </summary>
internal sealed class GetCurrencyByIdValidator : AbstractValidator<GetCurrencyByIdQuery>
{
    public GetCurrencyByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Currency ID must be greater than 0.");
    }
}

