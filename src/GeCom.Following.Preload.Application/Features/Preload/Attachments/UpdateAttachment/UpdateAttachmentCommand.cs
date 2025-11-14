using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Attachments;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.UpdateAttachment;

/// <summary>
/// Command to update an existing attachment.
/// </summary>
public sealed record UpdateAttachmentCommand(
    int AdjuntoId,
    string Path) : ICommand<AttachmentResponse>;

