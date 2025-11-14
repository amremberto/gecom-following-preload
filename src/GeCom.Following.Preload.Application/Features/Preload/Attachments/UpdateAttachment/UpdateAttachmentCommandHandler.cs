using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.UpdateAttachment;

/// <summary>
/// Handler for the UpdateAttachmentCommand.
/// </summary>
internal sealed class UpdateAttachmentCommandHandler : ICommandHandler<UpdateAttachmentCommand, AttachmentResponse>
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAttachmentCommandHandler"/> class.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateAttachmentCommandHandler(
        IAttachmentRepository attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<AttachmentResponse>> Handle(UpdateAttachmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el attachment existe
        Attachment? attachment = await _attachmentRepository.GetByIdAsync(request.AdjuntoId, cancellationToken);
        if (attachment is null)
        {
            return Result.Failure<AttachmentResponse>(
                Error.NotFound(
                    "Attachment.NotFound",
                    $"Attachment with ID '{request.AdjuntoId}' was not found."));
        }

        // Actualizar los campos
        attachment.Path = request.Path;
        attachment.FechaModificacion = DateTime.UtcNow;

        // Actualizar en el repositorio
        Attachment updatedAttachment = await _attachmentRepository.UpdateAsync(attachment, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        AttachmentResponse response = AttachmentMappings.ToResponse(updatedAttachment);

        return Result.Success(response);
    }
}

