using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.DeleteDocument;

/// <summary>
/// Handler for the DeleteDocumentCommand. Performs logical delete by setting FechaBaja.
/// </summary>
internal sealed class DeleteDocumentCommandHandler : ICommandHandler<DeleteDocumentCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (document is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        if (document.FechaBaja.HasValue)
        {
            return Result.Failure<DocumentResponse>(
                Error.Conflict(
                    "Document.AlreadyDeleted",
                    $"Document with ID '{request.DocId}' has already been logically deleted (FechaBaja is set)."));
        }

        // Solo se pueden eliminar documentos en estado PendPrecarga (1) o Precargado (2)
        if (document.EstadoId is not 1 and not 2)
        {
            string estadoDescripcion = document.State?.Descripcion ?? document.EstadoId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "desconocido";
            return Result.Failure<DocumentResponse>(
                Error.Conflict(
                    "Document.InvalidStateForDelete",
                    $"No se puede eliminar un documento en estado {estadoDescripcion}. Solo se permite eliminar documentos en estado Pendiente Precarga o Precarga Pendiente."));
        }

        document.FechaBaja = DateTime.UtcNow;

        Document updatedDocument = await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        DocumentResponse response = DocumentMappings.ToResponse(updatedDocument);
        return Result.Success(response);
    }
}
