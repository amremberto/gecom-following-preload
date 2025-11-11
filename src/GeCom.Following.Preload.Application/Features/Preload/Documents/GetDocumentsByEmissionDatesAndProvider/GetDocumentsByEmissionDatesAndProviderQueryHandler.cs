using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDatesAndProvider;

/// <summary>
/// Handler for the GetDocumentsByFechaEmisionAndProveedorQuery.
/// </summary>
internal sealed class GetDocumentsByEmissionDatesAndProviderQueryHandler
    : IQueryHandler<GetDocumentsByEmissionDatesAndProviderQuery, IEnumerable<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentsByEmissionDatesAndProviderQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    public GetDocumentsByEmissionDatesAndProviderQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentResponse>>> Handle(
        GetDocumentsByEmissionDatesAndProviderQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Document> documents =
            await _documentRepository.GetByEmissionDatesAndProviderCuitAsync(
                request.DateFrom,
                request.DateTo,
                request.ProviderCuit,
                cancellationToken);

        IEnumerable<DocumentResponse> response = documents.Select(DocumentMappings.ToResponse);

        return Result.Success(response);
    }
}

