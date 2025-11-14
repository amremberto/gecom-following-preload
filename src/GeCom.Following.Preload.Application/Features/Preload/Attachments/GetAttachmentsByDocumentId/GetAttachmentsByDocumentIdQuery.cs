using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Attachments;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAttachmentsByDocumentId;

/// <summary>
/// Query to get all attachments for a specific document.
/// </summary>
public sealed record GetAttachmentsByDocumentIdQuery(int DocId) : IQuery<IEnumerable<AttachmentResponse>>;

