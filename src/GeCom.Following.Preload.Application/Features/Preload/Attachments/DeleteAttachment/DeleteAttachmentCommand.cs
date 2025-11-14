using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.DeleteAttachment;

/// <summary>
/// Command to delete an attachment by its ID.
/// </summary>
public sealed record DeleteAttachmentCommand(int AdjuntoId) : ICommand;

