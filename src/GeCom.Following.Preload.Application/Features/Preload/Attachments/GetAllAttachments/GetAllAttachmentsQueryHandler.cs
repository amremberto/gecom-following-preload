using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAllAttachments;

/// <summary>
/// Handler for the GetAllAttachmentsQuery.
/// </summary>
internal sealed class GetAllAttachmentsQueryHandler : IQueryHandler<GetAllAttachmentsQuery, IEnumerable<AttachmentResponse>>
{
    private readonly IAttachmentRepository _attachmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllAttachmentsQueryHandler"/> class.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    public GetAllAttachmentsQueryHandler(IAttachmentRepository attachmentRepository)
    {
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<AttachmentResponse>>> Handle(GetAllAttachmentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Preloads.Attachments.Attachment> attachments = await _attachmentRepository.GetAllAsync(cancellationToken);

        IEnumerable<AttachmentResponse> response = attachments
            .OrderByDescending(a => a.FechaCreacion)
            .Select(AttachmentMappings.ToResponse);

        return Result.Success(response);
    }
}

