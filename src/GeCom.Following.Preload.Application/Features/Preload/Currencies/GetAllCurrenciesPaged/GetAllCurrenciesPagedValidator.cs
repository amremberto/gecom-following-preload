using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrenciesPaged;

/// <summary>
/// Validator for the GetAllCurrenciesPagedQuery.
/// </summary>
internal sealed class GetAllCurrenciesPagedValidator : AbstractValidator<GetAllCurrenciesPagedQuery>
{
    public GetAllCurrenciesPagedValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(200)
            .WithMessage("Page size must not exceed 200.");
    }
}

