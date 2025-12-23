using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocumentPdf;

/// <summary>
/// Handler for the UpdateDocumentPdfCommand.
/// </summary>
internal sealed class UpdateDocumentPdfCommandHandler : ICommandHandler<UpdateDocumentPdfCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentPdfCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="attachmentRepository">The attachment repository.</param>
    /// <param name="storageService">The storage service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateDocumentPdfCommandHandler(
        IDocumentRepository documentRepository,
        IAttachmentRepository attachmentRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(UpdateDocumentPdfCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType);

        // Validate file
        if (request.FileContent is null || request.FileContent.Length == 0)
        {
            return Result.Failure<DocumentResponse>(
                new Error(
                    "File.Required",
                    "A PDF file is required.",
                    ErrorType.Validation));
        }

        // Validate file type (using ToUpperInvariant for comparison per CA1308)
        string contentType = request.ContentType.ToUpperInvariant();
        string fileName = request.FileName;
        string fileExtension = Path.GetExtension(fileName).ToUpperInvariant();

        if (contentType != "APPLICATION/PDF" && fileExtension != ".PDF")
        {
            return Result.Failure<DocumentResponse>(
                new Error(
                    "File.InvalidType",
                    "Only PDF files are allowed.",
                    ErrorType.Validation));
        }

        // 1. Verificar que el documento existe
        Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (document is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        // 2. Obtener el adjunto activo (FechaBorrado == NULL) del documento con tracking habilitado
        Attachment? activeAttachment = await _attachmentRepository.GetActiveAttachmentByDocumentIdAsync(request.DocId, cancellationToken);

        if (activeAttachment is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Attachment.NotFound",
                    $"No active attachment found for document with ID '{request.DocId}'."));
        }

        // 3. Generar un GUID v7 para garantizar unicidad del nuevo archivo
        var codigoHash = Guid.CreateVersion7();

        // Generar nombre único usando el codigoHash
        string uniqueFileName = $"{codigoHash:N}.pdf";

        // 4. Guardar el nuevo archivo al storage primero
        // Si falla, retornar error inmediatamente sin modificar registros en la base de datos
        string newFilePath;
        try
        {
            newFilePath = await _storageService.SaveFileAsync(request.FileContent, uniqueFileName, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<DocumentResponse>(
                Error.Failure(
                    "Storage.UploadFailed",
                    $"Failed to upload new file to storage: {ex.Message}"));
        }

        // 5. Una vez subido el nuevo archivo exitosamente, marcar el adjunto activo como borrado
        DateTime utcNow = DateTime.UtcNow;
        activeAttachment.FechaBorrado = utcNow;
        activeAttachment.FechaModificacion = utcNow;

        // 6. Crear un nuevo registro de Attachment con el nuevo archivo
        Attachment newAttachment = new()
        {
            Path = newFilePath,
            DocId = request.DocId,
            FechaCreacion = utcNow,
            FechaModificacion = utcNow,
            FechaBorrado = null // Este es el adjunto activo
        };

        // Agregar el nuevo attachment al repositorio
        _ = await _attachmentRepository.AddAsync(newAttachment, cancellationToken);

        // 7. Guardar cambios en la base de datos (marca el antiguo como borrado y crea el nuevo)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Cargar el documento con sus relaciones para el mapeo
        Document? documentWithRelations = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (documentWithRelations is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found after update."));
        }

        // 9. Mapear a Response
        DocumentResponse response = DocumentMappings.ToResponse(documentWithRelations);

        return Result.Success(response);
    }
}

