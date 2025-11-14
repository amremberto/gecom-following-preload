using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.DeleteAttachment;

/// <summary>
/// Handler for the DeleteAttachmentCommand.
/// </summary>
internal sealed class DeleteAttachmentCommandHandler : ICommandHandler<DeleteAttachmentCommand>
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAttachmentCommandHandler"/> class.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteAttachmentCommandHandler(
        IAttachmentRepository attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el attachment existe
        Attachment? attachment = await _attachmentRepository.GetByIdAsync(request.AdjuntoId, cancellationToken);
        if (attachment is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "Attachment.NotFound",
                    $"Attachment with ID '{request.AdjuntoId}' was not found."));
        }

        // Eliminar el attachment
        await _attachmentRepository.RemoveByIdAsync(request.AdjuntoId, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

