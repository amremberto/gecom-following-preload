namespace GeCom.Following.Preload.Contracts.Preload.Attachments.Create;

/// <summary>
/// Request DTO for creating a new attachment.
/// </summary>
public sealed record CreateAttachmentRequest(
    string Path,
    int DocId
);

