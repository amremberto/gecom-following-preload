using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAttachmentById;

/// <summary>
/// Handler for the GetAttachmentByIdQuery.
/// </summary>
internal sealed class GetAttachmentByIdQueryHandler : IQueryHandler<GetAttachmentByIdQuery, AttachmentResponse>
{
    private readonly IAttachmentRepository _attachmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAttachmentByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    public GetAttachmentByIdQueryHandler(IAttachmentRepository attachmentRepository)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<AttachmentResponse>> Handle(GetAttachmentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Attachment? attachment = await _attachmentRepository.GetByIdAsync(request.AdjuntoId, cancellationToken);

        if (attachment is null)
        {
            return Result.Failure<AttachmentResponse>(
                Error.NotFound(
                    "Attachment.NotFound",
                    $"Attachment with ID '{request.AdjuntoId}' was not found."));
        }

        AttachmentResponse response = AttachmentMappings.ToResponse(attachment);

        return Result.Success(response);
    }
}

