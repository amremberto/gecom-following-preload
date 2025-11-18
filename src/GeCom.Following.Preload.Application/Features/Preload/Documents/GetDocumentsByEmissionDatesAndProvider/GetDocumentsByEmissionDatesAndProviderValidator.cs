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

        RuleFor(x => x.ProviderCuit)
            .NotEmpty()
            .WithMessage("Provider CUIT is required.")
            .NotNull()
            .WithMessage("Provider CUIT cannot be null.");
    }
}
