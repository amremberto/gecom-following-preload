using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.LinkSapPurchaseOrderToDocument;

/// <summary>
/// Command to link a SAP purchase order to a preload document.
/// </summary>
public sealed record LinkSapPurchaseOrderToDocumentCommand(
    int Ocid,
    string? CodigoRecepcion,
    decimal? CantidadAFacturar,
    int DocId,
    int OrdenCompraId,
    string NroOc,
    int PosicionOc,
    string CodigoSociedadFi,
    string ProveedorSap,
    decimal? ImporteNetoAnticipo) : ICommand<LinkSapPurchaseOrderToDocumentResponse>;
