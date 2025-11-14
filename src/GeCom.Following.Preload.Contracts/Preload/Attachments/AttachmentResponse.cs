namespace GeCom.Following.Preload.Contracts.Preload.Attachments;

/// <summary>
/// Response DTO for Attachment.
/// </summary>
public sealed record AttachmentResponse(
    int AdjuntoId,
    string Path,
    DateTime FechaCreacion,
    DateTime? FechaModificacion,
    DateTime? FechaBorrado,
    int DocId
);

