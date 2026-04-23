using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.LinkSapPurchaseOrderToDocument;

/// <summary>
/// Validator for LinkSapPurchaseOrderToDocumentCommand.
/// </summary>
internal sealed class LinkSapPurchaseOrderToDocumentCommandValidator : AbstractValidator<LinkSapPurchaseOrderToDocumentCommand>
{
    public LinkSapPurchaseOrderToDocumentCommandValidator()
    {
        RuleFor(x => x.Ocid)
            .GreaterThan(0)
            .WithMessage("Ocid must be greater than 0.");

        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");

        RuleFor(x => x.OrdenCompraId)
            .GreaterThan(0)
            .WithMessage("OrdenCompraId must be greater than 0.");

        RuleFor(x => x.PosicionOc)
            .GreaterThan(0)
            .WithMessage("PosicionOc must be greater than 0.");

        RuleFor(x => x.NroOc)
            .NotEmpty()
            .WithMessage("NroOc is required.")
            .MaximumLength(20)
            .WithMessage("NroOc must not exceed 20 characters.");

        RuleFor(x => x.CodigoSociedadFi)
            .NotEmpty()
            .WithMessage("CodigoSociedadFi is required.")
            .MaximumLength(10)
            .WithMessage("CodigoSociedadFi must not exceed 10 characters.");

        RuleFor(x => x.ProveedorSap)
            .NotEmpty()
            .WithMessage("ProveedorSap is required.")
            .MaximumLength(20)
            .WithMessage("ProveedorSap must not exceed 20 characters.");

        RuleFor(x => x.CodigoRecepcion)
            .MaximumLength(50)
            .WithMessage("CodigoRecepcion must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoRecepcion));

        RuleFor(x => x.CantidadAFacturar)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CantidadAFacturar must be greater than or equal to 0.")
            .When(x => x.CantidadAFacturar.HasValue);

        RuleFor(x => x.ImporteNetoAnticipo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ImporteNetoAnticipo must be greater than or equal to 0.")
            .When(x => x.ImporteNetoAnticipo.HasValue);

        RuleFor(x => x)
            .Must(HasValidLinkBusinessData)
            .WithMessage("Provide either CantidadAFacturar with CodigoRecepcion, or ImporteNetoAnticipo for advance invoices.");
    }

    private static bool HasValidLinkBusinessData(LinkSapPurchaseOrderToDocumentCommand command)
    {
        bool hasStandardLinkData = command.CantidadAFacturar.HasValue &&
                                   !string.IsNullOrWhiteSpace(command.CodigoRecepcion);
        bool hasAdvanceData = command.ImporteNetoAnticipo.HasValue;

        return hasStandardLinkData ^ hasAdvanceData;
    }
}
