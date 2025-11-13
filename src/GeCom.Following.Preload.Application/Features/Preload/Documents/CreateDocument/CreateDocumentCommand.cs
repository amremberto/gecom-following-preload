using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.CreateDocument;

/// <summary>
/// Command to create a new document.
/// </summary>
public sealed record CreateDocumentCommand(
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
    string? NombreSolicitante) : ICommand<DocumentResponse>;

