using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Attachments;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.CreateAttachment;

/// <summary>
/// Command to create a new attachment.
/// </summary>
public sealed record CreateAttachmentCommand(
    string Path,
    int DocId) : ICommand<AttachmentResponse>;

