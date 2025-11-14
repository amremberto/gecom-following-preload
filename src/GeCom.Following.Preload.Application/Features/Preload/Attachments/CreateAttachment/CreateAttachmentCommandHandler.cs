using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.CreateAttachment;

/// <summary>
/// Handler for the CreateAttachmentCommand.
/// </summary>
internal sealed class CreateAttachmentCommandHandler : ICommandHandler<CreateAttachmentCommand, AttachmentResponse>
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAttachmentCommandHandler"/> class.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateAttachmentCommandHandler(
        IAttachmentRepository attachmentRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<AttachmentResponse>> Handle(CreateAttachmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el documento existe
        Domain.Preloads.Documents.Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (document is null)
        {
            return Result.Failure<AttachmentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        // Crear la nueva entidad Attachment
        Attachment attachment = new()
        {
            Path = request.Path,
            DocId = request.DocId,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar al repositorio
        Attachment addedAttachment = await _attachmentRepository.AddAsync(attachment, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        AttachmentResponse response = AttachmentMappings.ToResponse(addedAttachment);

        return Result.Success(response);
    }
}

