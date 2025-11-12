using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.SearchProviders;

/// <summary>
/// Validator for SearchProvidersQuery.
/// </summary>
internal sealed class SearchProvidersValidator : AbstractValidator<SearchProvidersQuery>
{
    public SearchProvidersValidator()
    {
        RuleFor(x => x.SearchText)
            .NotEmpty()
            .WithMessage("Search text is required.");
    }
}
