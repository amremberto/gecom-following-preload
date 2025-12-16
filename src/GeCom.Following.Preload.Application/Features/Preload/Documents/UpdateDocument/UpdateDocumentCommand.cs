using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocument;

/// <summary>
/// Command to update an existing document.
/// </summary>
public sealed record UpdateDocumentCommand(
    int DocId,
    string UserEmail,
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
    string? NombreSolicitante) : ICommand<DocumentResponse>;
