using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.PreloadDocument;

/// <summary>
/// Handler for the PreloadDocumentCommand.
/// </summary>
internal sealed class PreloadDocumentCommandHandler : ICommandHandler<PreloadDocumentCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IDocumentStateRepository _documentStateRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreloadDocumentCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="attachmentRepository">The attachment repository.</param>
    /// <param name="documentStateRepository">The document state repository.</param>
    /// <param name="storageService">The storage service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public PreloadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IStateRepository stateRepository,
        IAttachmentRepository attachmentRepository,
        IDocumentStateRepository documentStateRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _documentStateRepository = documentStateRepository ?? throw new ArgumentNullException(nameof(documentStateRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(PreloadDocumentCommand request, CancellationToken cancellationToken)
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
                    "A PDF file is required for preloading.",
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

        // Generar un GUID v7 para garantizar unicidad del archivo
        var codigoHash = Guid.CreateVersion7();

        // Generar nombre único usando solo el codigoHash (sin DocId porque aún no existe)
        // El formato "N" del GUID ya genera en minúscula, por lo que no necesitamos ToLowerInvariant()
        string uniqueFileName = $"{codigoHash:N}.pdf";

        // 1. Intentar subir el archivo al storage primero
        // Si falla, retornar error inmediatamente sin crear registros en la base de datos
        string filePath;
        try
        {
            filePath = await _storageService.SaveFileAsync(request.FileContent, uniqueFileName, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<DocumentResponse>(
                Error.Failure(
                    "Storage.UploadFailed",
                    $"Failed to upload file to storage: {ex.Message}"));
        }

        // 2. Una vez subido el archivo exitosamente, crear las entidades en la base de datos

        // Buscar el estado con código "PendPrecarga"
        State? preloadState = await _stateRepository.GetByCodeAsync("PendPrecarga", cancellationToken);
        if (preloadState is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "State.NotFound",
                    "State with code 'PendPrecarga' was not found. Please ensure the state exists in the database."));
        }

        // Crear el documento con valores por defecto
        Document document = new()
        {
            EstadoId = preloadState.EstadoId,
            FechaCreacion = DateTime.UtcNow,
            FechaEmisionComprobante = DateOnly.FromDateTime(DateTime.UtcNow),
            UserCreate = request.UserEmail,
            ProveedorCuit = request.ProviderCuit
        };

        // Agregar el documento al repositorio
        Document addedDocument = await _documentRepository.AddAsync(document, cancellationToken);

        // Guardar cambios para obtener el DocId
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Crear el registro de DocumentState que relaciona el documento con su estado
        DocumentState documentState = new()
        {
            DocId = addedDocument.DocId,
            EstadoId = preloadState.EstadoId,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar el DocumentState al repositorio
        _ = await _documentStateRepository.AddAsync(documentState, cancellationToken);

        // Crear el registro de Attachment que relaciona el documento con el archivo subido
        Attachment attachment = new()
        {
            Path = filePath,
            DocId = addedDocument.DocId,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar el attachment al repositorio
        _ = await _attachmentRepository.AddAsync(attachment, cancellationToken);

        // Guardar cambios finales (DocumentState y Attachment)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cargar el documento con sus relaciones para el mapeo
        Document? documentWithRelations = await _documentRepository.GetByIdAsync(addedDocument.DocId, cancellationToken);
        if (documentWithRelations is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{addedDocument.DocId}' was not found after creation."));
        }

        // Mapear a Response
        DocumentResponse response = DocumentMappings.ToResponse(documentWithRelations);

        return Result.Success(response);
    }
}

