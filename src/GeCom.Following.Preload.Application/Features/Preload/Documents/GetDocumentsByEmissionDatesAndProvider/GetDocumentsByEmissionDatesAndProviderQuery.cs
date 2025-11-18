using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDatesAndProvider;

/// <summary>
/// Query to get documents by emission date range and provider CUIT.
/// </summary>
public sealed record GetDocumentsByEmissionDatesAndProviderQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    string ProviderCuit
) : IQuery<IEnumerable<DocumentResponse>>;
