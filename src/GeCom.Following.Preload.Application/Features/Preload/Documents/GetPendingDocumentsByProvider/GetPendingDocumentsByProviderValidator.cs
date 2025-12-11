using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocumentsByProvider;

/// <summary>
/// Validator for GetPendingDocumentsByProviderQuery.
/// </summary>
internal sealed class GetPendingDocumentsByProviderValidator
    : AbstractValidator<GetPendingDocumentsByProviderQuery>
{
    public GetPendingDocumentsByProviderValidator()
    {
        RuleFor(x => x.ProviderCuit)
            .NotEmpty()
            .WithMessage("Provider CUIT is required.")
            .NotNull()
            .WithMessage("Provider CUIT cannot be null.");
    }
}

