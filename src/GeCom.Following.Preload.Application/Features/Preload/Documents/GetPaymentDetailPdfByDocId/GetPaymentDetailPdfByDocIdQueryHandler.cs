using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaymentDetailPdfByDocId;

/// <summary>
/// Handler for GetPaymentDetailPdfByDocIdQuery. Resolves document → IdDetalleDePago → PaymentDetail, then reads PDF from storage.
/// </summary>
internal sealed class GetPaymentDetailPdfByDocIdQueryHandler : IQueryHandler<GetPaymentDetailPdfByDocIdQuery, PaymentDetailPdfResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IPaymentDetailRepository _paymentDetailRepository;
    private readonly IStorageService _storageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPaymentDetailPdfByDocIdQueryHandler"/> class.
    /// </summary>
    public GetPaymentDetailPdfByDocIdQueryHandler(
        IDocumentRepository documentRepository,
        IPaymentDetailRepository paymentDetailRepository,
        IStorageService storageService)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _paymentDetailRepository = paymentDetailRepository ?? throw new ArgumentNullException(nameof(paymentDetailRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentDetailPdfResult>> Handle(GetPaymentDetailPdfByDocIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.Documents.Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);

        if (document is null)
        {
            return Result.Failure<PaymentDetailPdfResult>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        if (!document.IdDetalleDePago.HasValue)
        {
            return Result.Failure<PaymentDetailPdfResult>(
                Error.NotFound(
                    "Document.PaymentNotConfirmed",
                    "The document does not have a confirmed payment (no payment detail associated)."));
        }

        Domain.Preloads.PaymentDetails.PaymentDetail? paymentDetail =
            await _paymentDetailRepository.GetByIdAsync(document.IdDetalleDePago.Value, cancellationToken);

        if (paymentDetail is null)
        {
            return Result.Failure<PaymentDetailPdfResult>(
                Error.NotFound(
                    "PaymentDetail.NotFound",
                    $"Payment detail with ID '{document.IdDetalleDePago.Value}' was not found."));
        }

        try
        {
            byte[] content = await _storageService.ReadPaymentDetailFileAsync(
                paymentDetail.NamePdf,
                paymentDetail.FechaAlta.Year,
                paymentDetail.FechaAlta.Month,
                cancellationToken);

            var pdfResult = new PaymentDetailPdfResult(content, paymentDetail.NamePdf);
            return Result.Success(pdfResult);
        }
        catch (FileNotFoundException)
        {
            return Result.Failure<PaymentDetailPdfResult>(
                Error.NotFound(
                    "PaymentDetail.FileNotFound",
                    $"Payment detail PDF file '{paymentDetail.NamePdf}' was not found in storage."));
        }
    }
}
