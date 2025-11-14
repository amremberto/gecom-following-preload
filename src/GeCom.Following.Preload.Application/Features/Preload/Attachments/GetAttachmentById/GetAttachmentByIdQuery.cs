using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Attachments;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAttachmentById;

/// <summary>
/// Query to get an attachment by its ID.
/// </summary>
public sealed record GetAttachmentByIdQuery(int AdjuntoId) : IQuery<AttachmentResponse>;

