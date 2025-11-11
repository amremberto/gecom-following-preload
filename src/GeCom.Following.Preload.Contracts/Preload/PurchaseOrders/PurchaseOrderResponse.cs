namespace GeCom.Following.Preload.Contracts.Preload.PurchaseOrders;

/// <summary>
/// Response DTO for PurchaseOrder.
/// </summary>
public sealed record PurchaseOrderResponse(
    int Ocid,
    string? CodigoRecepcion,
    decimal CantidadAfacturar,
    int DocId,
    int OrdenCompraId,
    DateTime FechaCreacion,
    DateTime? FechaBaja,
    string NroOc,
    int PosicionOc,
    string CodigoSociedadFi,
    string ProveedorSap
);

