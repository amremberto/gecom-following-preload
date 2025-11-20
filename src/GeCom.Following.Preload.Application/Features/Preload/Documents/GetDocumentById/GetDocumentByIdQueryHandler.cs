using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentById;

/// <summary>
/// Handler for the GetDocumentByIdQuery.
/// </summary>
internal sealed class GetDocumentByIdQueryHandler : IQueryHandler<GetDocumentByIdQuery, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    public GetDocumentByIdQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.Documents.Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);

        if (document is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        DocumentResponse response = DocumentMappings.ToResponse(document);

        return Result.Success(response);
    }
}

