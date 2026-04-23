using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.UnlinkSapPurchaseOrderFromDocument;

/// <summary>
/// Command to unlink a SAP purchase order from a preload document.
/// </summary>
public sealed record UnlinkSapPurchaseOrderFromDocumentCommand(
    int DocId,
    int Posicion,
    string NumeroDocumento,
    string CodigoRecepcion) : ICommand;
