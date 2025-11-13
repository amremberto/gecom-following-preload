using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetAllDocumentTypes;

/// <summary>
/// Handler for the GetAllDocumentTypesQuery.
/// </summary>
internal sealed class GetAllDocumentTypesQueryHandler : IQueryHandler<GetAllDocumentTypesQuery, IEnumerable<DocumentTypeResponse>>
{
    private readonly IDocumentTypeRepository _documentTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllDocumentTypesQueryHandler"/> class.
    /// </summary>
    /// <param name="documentTypeRepository">The document type repository.</param>
    public GetAllDocumentTypesQueryHandler(IDocumentTypeRepository documentTypeRepository)
    {
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentTypeResponse>>> Handle(GetAllDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<DocumentType> documentTypes = await _documentTypeRepository.GetAllAsync(cancellationToken);

        IEnumerable<DocumentTypeResponse> response = documentTypes.Select(DocumentTypeMappings.ToResponse);

        return Result.Success(response);
    }
}

