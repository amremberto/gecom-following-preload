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
            .WithMessage("FechaDesde is required.");

        RuleFor(x => x.DateTo)
            .NotEmpty()
            .WithMessage("FechaHasta is required.")
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("FechaHasta must be greater than or equal to FechaDesde.");

        RuleFor(x => x.ProviderCuit)
            .NotEmpty()
            .WithMessage("ProveedorCuit is required.");
    }
}

