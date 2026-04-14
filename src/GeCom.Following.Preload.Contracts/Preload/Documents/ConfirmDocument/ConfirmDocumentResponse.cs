namespace GeCom.Following.Preload.Contracts.Preload.Documents.ConfirmDocument;

/// <summary>
/// Response returned when a document is confirmed and sent to mesa de entrada.
/// </summary>
/// <param name="DocId">Document identifier.</param>
/// <param name="Message">Operation result message.</param>
public sealed record ConfirmDocumentResponse(
    int DocId,
    string Message);
