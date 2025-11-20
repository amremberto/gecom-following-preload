using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Contracts.Preload.PurchaseOrders;

namespace GeCom.Following.Preload.Contracts.Preload.Documents;

/// <summary>
/// Response DTO for Document.
/// </summary>
public sealed record DocumentResponse(
    int DocId,
    string? ProveedorCuit,
    string? ProveedorRazonSocial,
    string? SociedadCuit,
    string? SociedadDescripcion,
    int? TipoDocId,
    string? TipoDocDescripcion,
    string? PuntoDeVenta,
    string? NumeroComprobante,
    DateOnly? FechaEmisionComprobante,
    string? Moneda,
    decimal? MontoBruto,
    string? CodigoDeBarras,
    string? Caecai,
    DateOnly? VencimientoCaecai,
    int? EstadoId,
    string? EstadoDescripcion,
    DateTime? FechaCreacion,
    DateTime? FechaBaja,
    int? IdDocument,
    string? NombreSolicitante,
    int? IdDetalleDePago,
    int? IdMetodoDePago,
    DateTime? FechaPago,
    string? UserCreate,
    ICollection<PurchaseOrderResponse> PurchaseOrders,
    ICollection<NoteResponse> Notes,
    ICollection<AttachmentResponse>? Attachments = null
);

