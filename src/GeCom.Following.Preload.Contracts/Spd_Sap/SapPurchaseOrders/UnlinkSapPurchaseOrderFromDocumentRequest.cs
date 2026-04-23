using System.Text.Json.Serialization;

namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Request DTO to unlink a SAP purchase order from a preload document.
/// Mirrors the legacy OCUnlinkRequest used by api/oc/unlink-document.
/// </summary>
public sealed record UnlinkSapPurchaseOrderFromDocumentRequest(
    [property: JsonPropertyName("docId")] int DocId,
    [property: JsonPropertyName("posicion")] int Posicion,
    [property: JsonPropertyName("numeroDocumento")] string NumeroDocumento,
    [property: JsonPropertyName("codigoRecepcion")] string CodigoRecepcion);
