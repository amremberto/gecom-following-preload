using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDatesAndProvider;

/// <summary>
/// Validator for GetDocumentsByEmissionDatesAndProviderQuery.
/// </summary>
internal sealed class GetDocumentsByEmissionDatesAndProviderValidator
    : AbstractValidator<GetDocumentsByEmissionDatesAndProviderQuery>
{
    public GetDocumentsByEmissionDatesAndProviderValidator()
    {
        RuleFor(x => x.DateFrom)
            .NotEmpty()
            .WithMessage("DateFrom is required.");

        RuleFor(x => x.DateTo)
            .NotEmpty()
            .WithMessage("DateTo is required.")
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("DateTo must be greater than or equal to DateFrom.");

        // ProviderCuit is optional, so no validation needed
        // If provided, it should not be empty (handled by the repository logic)
    }
}
