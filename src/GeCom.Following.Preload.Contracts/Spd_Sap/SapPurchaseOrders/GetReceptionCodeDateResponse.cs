namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Response DTO containing the reception code date information for a SAP purchase order.
/// </summary>
public sealed record GetReceptionCodeDateResponse(
    long? OrdenCompraId,
    string CodigoRecepcion,
    DateTime FechaCodigoRecepcion);

