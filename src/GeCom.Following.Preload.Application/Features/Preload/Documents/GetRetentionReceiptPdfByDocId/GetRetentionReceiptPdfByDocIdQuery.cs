using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetRetentionReceiptPdfByDocId;

/// <summary>
/// Query to get the retention receipt (comprobante de retención) PDF for a document by its DocId.
/// </summary>
/// <param name="DocId">Document ID. The document must have an active attachment (preload PDF).</param>
public sealed record GetRetentionReceiptPdfByDocIdQuery(int DocId) : IQuery<RetentionReceiptPdfResult>;
