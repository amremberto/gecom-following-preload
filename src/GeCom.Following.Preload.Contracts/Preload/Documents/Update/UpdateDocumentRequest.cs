namespace GeCom.Following.Preload.Contracts.Preload.Documents.Update;

/// <summary>
/// Request DTO for updating an existing document.
/// </summary>
public sealed record UpdateDocumentRequest(
    int DocId,
    string? ProveedorCuit,
    string? SociedadCuit,
    int? TipoDocId,
    string? PuntoDeVenta,
    string? NumeroComprobante,
    DateOnly? FechaEmisionComprobante,
    string? Moneda,
    decimal? MontoBruto,
    string? CodigoDeBarras,
    string? Caecai,
    DateOnly? VencimientoCaecai,
    int? EstadoId,
    string? NombreSolicitante
);
