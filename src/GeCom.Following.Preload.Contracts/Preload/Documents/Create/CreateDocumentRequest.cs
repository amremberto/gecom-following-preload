namespace GeCom.Following.Preload.Contracts.Preload.Documents.Create;

/// <summary>
/// Request DTO for creating a new document.
/// </summary>
public sealed record CreateDocumentRequest(
    string ProveedorCuit,
    string SociedadCuit,
    int? TipoDocId,
    string? PuntoDeVenta,
    string? NumeroComprobante,
    DateOnly? FechaEmisionComprobante,
    string? Moneda,
    decimal? MontoBruto,
    string? Caecai,
    DateOnly? VencimientoCaecai,
    string? NombreSolicitante
);

