using System.Text.Json.Serialization;

namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Response DTO for a successful link between a SAP purchase order and a preload document.
/// </summary>
public sealed record LinkSapPurchaseOrderToDocumentResponse(
    [property: JsonPropertyName("ocid")] int Ocid,
    [property: JsonPropertyName("docId")] int DocId,
    [property: JsonPropertyName("ordenCompraId")] int OrdenCompraId,
    [property: JsonPropertyName("nroOc")] string NroOc,
    [property: JsonPropertyName("posicionOc")] int PosicionOc,
    [property: JsonPropertyName("codigoSociedadFi")] string CodigoSociedadFi,
    [property: JsonPropertyName("proveedorSap")] string ProveedorSap,
    [property: JsonPropertyName("codigoRecepcion")] string? CodigoRecepcion,
    [property: JsonPropertyName("cantidadAFacturar")] decimal CantidadAFacturar,
    [property: JsonPropertyName("fechaCreacion")] DateTime FechaCreacion);
