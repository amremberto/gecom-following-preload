using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaymentDetailPdfByDocId;

/// <summary>
/// Validator for GetPaymentDetailPdfByDocIdQuery.
/// </summary>
internal sealed class GetPaymentDetailPdfByDocIdValidator : AbstractValidator<GetPaymentDetailPdfByDocIdQuery>
{
    public GetPaymentDetailPdfByDocIdValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");
    }
}
