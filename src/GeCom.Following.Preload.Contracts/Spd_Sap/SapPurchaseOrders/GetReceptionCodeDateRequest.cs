namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Request DTO to retrieve the reception code date for a SAP purchase order.
/// Mirrors the intent of the legacy OCRequest used by api/oc/get-fecodigorecepcion.
/// </summary>
public sealed record GetReceptionCodeDateRequest(
    long? OrdenCompraId,
    string CodigoRecepcion);

