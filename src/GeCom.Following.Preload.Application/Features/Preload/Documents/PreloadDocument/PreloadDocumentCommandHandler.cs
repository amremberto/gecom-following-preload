using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Documents;
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
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreloadDocumentCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="attachmentRepository">The attachment repository.</param>
    /// <param name="storageService">The storage service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public PreloadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IStateRepository stateRepository,
        IAttachmentRepository attachmentRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(PreloadDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate file
        if (request.FileContent is null || request.FileContent.Length == 0)
        {
            return Result.Failure<DocumentResponse>(
                new Error(
                    "File.Required",
                    "A PDF file is required for preloading.",
                    ErrorType.Validation));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType);

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

        // Buscar el estado con código "Precargado"
        State? preloadState = await _stateRepository.GetByCodeAsync("PendPrecarga", cancellationToken);
        if (preloadState is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "State.NotFound",
                    "State with code 'Precargado' was not found. Please ensure the state exists in the database."));
        }

        Attachment addedAttachment = new();

        // Crear el documento con valores por defecto
        Document document = new()
        {
            EstadoId = preloadState.EstadoId,
            FechaCreacion = DateTime.UtcNow,
            FechaEmisionComprobante = DateOnly.FromDateTime(DateTime.UtcNow),
            UserCreate = request.UserEmail
        };

        // Agregar el documento al repositorio
        Document addedDocument = await _documentRepository.AddAsync(document, cancellationToken);

        // Guardar cambios para obtener el DocId
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            // Generar un GUID v7 para garantizar unicidad
            var codigoHash = Guid.CreateVersion7();

            // Generar nombre único en formato: DocId-CodigoHash.pdf
            // El formato "N" del GUID ya genera en minúscula, por lo que no necesitamos ToLowerInvariant()
            string uniqueFileName = $"{addedDocument.DocId}-{codigoHash:N}.pdf";

            // Guardar el archivo en el storage usando impersonación
            string filePath = await _storageService.SaveFileAsync(request.FileContent, uniqueFileName, cancellationToken);

            // Crear el registro de Attachment
            var attachment = new Attachment
            {
                Path = filePath,
                DocId = addedDocument.DocId,
                FechaCreacion = DateTime.UtcNow
            };

            attachment.Path = filePath;
            attachment.DocId = addedDocument.DocId;
            attachment.FechaCreacion = DateTime.UtcNow;

            // Agregar el attachment al repositorio
            addedAttachment = await _attachmentRepository.AddAsync(attachment, cancellationToken);

            // Guardar cambios finales
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
        catch (Exception ex)
        {
            // Si falla la operación de storage, intentar limpiar el documento creado
            // (opcional, dependiendo de los requisitos de negocio)
            await _attachmentRepository.RemoveByIdAsync(addedAttachment.AdjuntoId, cancellationToken);
            await _documentRepository.RemoveByIdAsync(addedDocument.DocId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<DocumentResponse>(
                Error.Failure(
                    "Preload.Failed",
                    $"Failed to preload document: {ex.Message}"));
        }
    }
}

