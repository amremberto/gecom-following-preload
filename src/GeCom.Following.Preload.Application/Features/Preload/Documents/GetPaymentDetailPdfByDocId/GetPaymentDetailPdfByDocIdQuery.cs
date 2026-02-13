using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaymentDetailPdfByDocId;

/// <summary>
/// Query to get the payment detail (recibo) PDF for a document by its DocId.
/// </summary>
/// <param name="DocId">Document ID. The document must have payment confirmed (IdDetalleDePago set).</param>
public sealed record GetPaymentDetailPdfByDocIdQuery(int DocId) : IQuery<PaymentDetailPdfResult>;
