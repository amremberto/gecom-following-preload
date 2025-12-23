using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrdersByProviderSocietyAndDocId;

/// <summary>
/// Validator for GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery.
/// </summary>
internal sealed class GetSapPurchaseOrdersByProviderSocietyAndDocIdQueryValidator
    : AbstractValidator<GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery>
{
    public GetSapPurchaseOrdersByProviderSocietyAndDocIdQueryValidator()
    {
        RuleFor(x => x.ProviderCuit)
            .NotEmpty()
            .WithMessage("Provider cuit is required.")
            .NotNull()
            .WithMessage("Provider cuit cannot be null.");

        RuleFor(x => x.SocietyCuit)
            .NotEmpty()
            .WithMessage("Society cuit is required.")
            .NotNull()
            .WithMessage("Society cuit cannot be null.");

        RuleFor(x => x.DocId)
            .NotEmpty()
            .WithMessage("Document Id is required.")
            .NotNull()
            .WithMessage("Document Id cannot be null.");
    }
}

