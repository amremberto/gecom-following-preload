namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaymentDetailPdfByDocId;

/// <summary>
/// Result of loading the payment detail PDF for a document.
/// </summary>
/// <param name="Content">PDF file content.</param>
/// <param name="FileName">Suggested file name for download (e.g. from PaymentDetail.NamePdf).</param>
public sealed record PaymentDetailPdfResult(byte[] Content, string FileName);
