using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetRetentionReceiptPdfByDocId;

/// <summary>
/// Validator for GetRetentionReceiptPdfByDocIdQuery.
/// </summary>
internal sealed class GetRetentionReceiptPdfByDocIdValidator : AbstractValidator<GetRetentionReceiptPdfByDocIdQuery>
{
    public GetRetentionReceiptPdfByDocIdValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");
    }
}
