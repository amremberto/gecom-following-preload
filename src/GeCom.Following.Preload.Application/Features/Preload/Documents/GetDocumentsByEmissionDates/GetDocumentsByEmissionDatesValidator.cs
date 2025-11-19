using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDates;

/// <summary>
/// Validator for GetDocumentsByEmissionDatesQuery.
/// </summary>
internal sealed class GetDocumentsByEmissionDatesValidator
    : AbstractValidator<GetDocumentsByEmissionDatesQuery>
{
    public GetDocumentsByEmissionDatesValidator()
    {
        RuleFor(x => x.DateFrom)
            .NotEmpty()
            .WithMessage("DateFrom is required.");

        RuleFor(x => x.DateTo)
            .NotEmpty()
            .WithMessage("DateTo is required.")
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .WithMessage("DateTo must be greater than or equal to DateFrom.");

        RuleFor(x => x.UserRoles)
            .NotNull()
            .WithMessage("User roles are required.")
            .NotEmpty()
            .WithMessage("At least one user role is required.");
    }
}

