using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.UnlinkSapPurchaseOrderFromDocument;

/// <summary>
/// Validator for UnlinkSapPurchaseOrderFromDocumentCommand.
/// </summary>
internal sealed class UnlinkSapPurchaseOrderFromDocumentCommandValidator : AbstractValidator<UnlinkSapPurchaseOrderFromDocumentCommand>
{
    public UnlinkSapPurchaseOrderFromDocumentCommandValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");

        RuleFor(x => x.Posicion)
            .GreaterThan(0)
            .WithMessage("Posicion must be greater than 0.");

        RuleFor(x => x.NumeroDocumento)
            .NotEmpty()
            .WithMessage("NumeroDocumento is required.")
            .MaximumLength(20)
            .WithMessage("NumeroDocumento must not exceed 20 characters.");

        RuleFor(x => x.CodigoRecepcion)
            .NotEmpty()
            .WithMessage("CodigoRecepcion is required.")
            .MaximumLength(50)
            .WithMessage("CodigoRecepcion must not exceed 50 characters.");
    }
}
