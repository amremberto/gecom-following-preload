using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.UpdateDocumentType;

/// <summary>
/// Handler for the UpdateDocumentTypeCommand.
/// </summary>
internal sealed class UpdateDocumentTypeCommandHandler : ICommandHandler<UpdateDocumentTypeCommand, DocumentTypeResponse>
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentTypeCommandHandler"/> class.
    /// </summary>
    /// <param name="documentTypeRepository">The document type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateDocumentTypeCommandHandler(IDocumentTypeRepository documentTypeRepository, IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentTypeResponse>> Handle(UpdateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el DocumentType existe
        DocumentType? documentType = await _documentTypeRepository.GetByTipoDocIdAsync(request.TipoDocId, cancellationToken);
        if (documentType is null)
        {
            return Result.Failure<DocumentTypeResponse>(
                Error.NotFound(
                    "DocumentType.NotFound",
                    $"Document type with ID '{request.TipoDocId}' was not found."));
        }

        // Verificar si ya existe otro DocumentType con el mismo código (excluyendo el actual)
        DocumentType? existingByCode = await _documentTypeRepository.GetByCodeAsync(request.Codigo, cancellationToken);
        if (existingByCode is not null && existingByCode.TipoDocId != request.TipoDocId)
        {
            return Result.Failure<DocumentTypeResponse>(
                Error.Conflict(
                    "DocumentType.Conflict",
                    $"A document type with code '{request.Codigo}' already exists."));
        }

        // Actualizar los campos
        documentType.Descripcion = request.Descripcion;
        documentType.Letra = request.Letra;
        documentType.Codigo = request.Codigo;
        documentType.DescripcionLarga = request.DescripcionLarga;
        documentType.IsFec = request.IsFec;

        // Actualizar en el repositorio
        DocumentType updatedDocumentType = await _documentTypeRepository.UpdateAsync(documentType, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        DocumentTypeResponse response = DocumentTypeMappings.ToResponse(updatedDocumentType);

        return Result.Success(response);
    }
}

