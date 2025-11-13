using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.DeleteDocumentType;

/// <summary>
/// Handler for the DeleteDocumentTypeCommand.
/// </summary>
internal sealed class DeleteDocumentTypeCommandHandler : ICommandHandler<DeleteDocumentTypeCommand>
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentTypeCommandHandler"/> class.
    /// </summary>
    /// <param name="documentTypeRepository">The document type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteDocumentTypeCommandHandler(IDocumentTypeRepository documentTypeRepository, IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el DocumentType existe
        DocumentType? documentType = await _documentTypeRepository.GetByTipoDocIdAsync(request.TipoDocId, cancellationToken);
        if (documentType is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "DocumentType.NotFound",
                    $"Document type with ID '{request.TipoDocId}' was not found."));
        }

        // Eliminar el DocumentType
        await _documentTypeRepository.RemoveByIdAsync(request.TipoDocId, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

