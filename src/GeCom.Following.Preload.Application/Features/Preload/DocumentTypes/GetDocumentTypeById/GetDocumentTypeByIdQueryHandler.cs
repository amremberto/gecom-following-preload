using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetDocumentTypeById;

/// <summary>
/// Handler for the GetDocumentTypeByIdQuery.
/// </summary>
internal sealed class GetDocumentTypeByIdQueryHandler : IQueryHandler<GetDocumentTypeByIdQuery, DocumentTypeResponse>
{
    private readonly IDocumentTypeRepository _documentTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentTypeByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="documentTypeRepository">The document type repository.</param>
    public GetDocumentTypeByIdQueryHandler(IDocumentTypeRepository documentTypeRepository)
    {
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentTypeResponse>> Handle(GetDocumentTypeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        DocumentType? documentType = await _documentTypeRepository.GetByTipoDocIdAsync(request.TipoDocId, cancellationToken);

        if (documentType is null)
        {
            return Result.Failure<DocumentTypeResponse>(
                Error.NotFound(
                    "DocumentType.NotFound",
                    $"Document type with ID '{request.TipoDocId}' was not found."));
        }

        DocumentTypeResponse response = DocumentTypeMappings.ToResponse(documentType);

        return Result.Success(response);
    }
}

