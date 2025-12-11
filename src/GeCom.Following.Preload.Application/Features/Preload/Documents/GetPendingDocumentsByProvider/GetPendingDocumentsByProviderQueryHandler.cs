using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocumentsByProvider;

/// <summary>
/// Handler for the GetPendingDocumentsByProviderQuery.
/// Returns documents with EstadoId == 2 or EstadoId == 5 that have FechaEmisionComprobante set.
/// </summary>
internal sealed class GetPendingDocumentsByProviderQueryHandler
    : IQueryHandler<GetPendingDocumentsByProviderQuery, IEnumerable<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPendingDocumentsByProviderQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    public GetPendingDocumentsByProviderQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentResponse>>> Handle(
        GetPendingDocumentsByProviderQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Document> documents =
            await _documentRepository.GetPendingDocumentsByProviderCuitAsync(
                request.ProviderCuit,
                cancellationToken);

        IEnumerable<DocumentResponse> response = documents.Select(DocumentMappings.ToResponse);

        return Result.Success(response);
    }
}

