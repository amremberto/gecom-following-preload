namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Response DTO for SAP purchase orders when dealing with credit or debit notes.
/// Includes additional fields from the Preload.OrdenesCompra table and calculated fields.
/// </summary>
public sealed record SapPurchaseOrderCreditDebitNoteResponse(
    long OrdenCompraId,
    string NumeroDocumento,
    int Posicion,
    string? DescripcionProducto,
    string? UnidadMedida,
    DateTime? FechaEmisionOC,
    decimal CantidadPedida,
    decimal CantidadRecepcionada,
    decimal CantidadFacturada,
    decimal CantidadFaltaFacturar,
    decimal? CantidadAFacturar,
    string? CodigoRecepcion,
    decimal ImporteOriginal,
    decimal? ImporteTotal,
    string? CodigoSociedadFI,
    string ProveedorSAP,
    string? Contacto,
    decimal? ImporteNetoAnticipo
);

