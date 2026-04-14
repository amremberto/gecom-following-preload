namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetRetentionReceiptPdfByDocId;

/// <summary>
/// Result of loading the retention receipt PDF for a document.
/// </summary>
/// <param name="Content">PDF file content.</param>
/// <param name="FileName">Suggested file name for download.</param>
public sealed record RetentionReceiptPdfResult(byte[] Content, string FileName);
