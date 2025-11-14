using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Attachments;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAllAttachments;

/// <summary>
/// Query to get all attachments.
/// </summary>
public sealed record GetAllAttachmentsQuery() : IQuery<IEnumerable<AttachmentResponse>>;

