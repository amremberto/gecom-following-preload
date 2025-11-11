using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetAllDocuments;

/// <summary>
/// Handler for the GetAllDocumentsQuery.
/// </summary>
internal sealed class GetAllDocumentsQueryHandler : IQueryHandler<GetAllDocumentsQuery, IEnumerable<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllDocumentsQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    public GetAllDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentResponse>>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Document> documents = await _documentRepository.GetAllAsync(cancellationToken);

        IEnumerable<DocumentResponse> response = documents.Select(DocumentMappings.ToResponse);

        return Result.Success(response);
    }
}

