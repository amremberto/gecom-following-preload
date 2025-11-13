using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetDocumentTypeByCode;

/// <summary>
/// Handler for the GetDocumentTypeByCodeQuery.
/// </summary>
internal sealed class GetDocumentTypeByCodeQueryHandler : IQueryHandler<GetDocumentTypeByCodeQuery, DocumentTypeResponse>
{
    private readonly IDocumentTypeRepository _documentTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentTypeByCodeQueryHandler"/> class.
    /// </summary>
    /// <param name="documentTypeRepository">The document type repository.</param>
    public GetDocumentTypeByCodeQueryHandler(IDocumentTypeRepository documentTypeRepository)
    {
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentTypeResponse>> Handle(GetDocumentTypeByCodeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        DocumentType? documentType = await _documentTypeRepository.GetByCodeAsync(request.Codigo, cancellationToken);

        if (documentType is null)
        {
            return Result.Failure<DocumentTypeResponse>(
                Error.NotFound(
                    "DocumentType.NotFound",
                    $"Document type with code '{request.Codigo}' was not found."));
        }

        DocumentTypeResponse response = DocumentTypeMappings.ToResponse(documentType);

        return Result.Success(response);
    }
}

