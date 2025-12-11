using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocumentsByProvider;

/// <summary>
/// Query to get pending documents by provider CUIT.
/// Pending documents are those with EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
/// </summary>
/// <param name="ProviderCuit">Provider CUIT to filter by.</param>
public sealed record GetPendingDocumentsByProviderQuery(
    string ProviderCuit
) : IQuery<IEnumerable<DocumentResponse>>;

