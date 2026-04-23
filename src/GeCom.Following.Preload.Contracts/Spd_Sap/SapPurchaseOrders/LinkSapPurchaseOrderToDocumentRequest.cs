using System.Text.Json.Serialization;

namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Request DTO to link a SAP purchase order to a preload document.
/// Mirrors the legacy OCRequest used by api/oc/link-document.
/// </summary>
public sealed record LinkSapPurchaseOrderToDocumentRequest(
    [property: JsonPropertyName("ocid")] int Ocid,
    [property: JsonPropertyName("codigoRecepcion")] string? CodigoRecepcion,
    [property: JsonPropertyName("cantidadAFacturar")] decimal? CantidadAFacturar,
    [property: JsonPropertyName("docId")] int DocId,
    [property: JsonPropertyName("ordenCompraId")] int OrdenCompraId,
    [property: JsonPropertyName("nroOc")] string NroOc,
    [property: JsonPropertyName("posicionOc")] int PosicionOc,
    [property: JsonPropertyName("codigoSociedadFi")] string CodigoSociedadFi,
    [property: JsonPropertyName("proveedorSap")] string ProveedorSap,
    [property: JsonPropertyName("importeNetoAnticipo")] decimal? ImporteNetoAnticipo);
