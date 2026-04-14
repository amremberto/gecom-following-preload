using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetRetentionReceiptPdfByDocId;

/// <summary>
/// Handler for GetRetentionReceiptPdfByDocIdQuery. Resolves document → active attachment, then reads PDF from storage.
/// </summary>
internal sealed class GetRetentionReceiptPdfByDocIdQueryHandler : IQueryHandler<GetRetentionReceiptPdfByDocIdQuery, RetentionReceiptPdfResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStorageService _storageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRetentionReceiptPdfByDocIdQueryHandler"/> class.
    /// </summary>
    public GetRetentionReceiptPdfByDocIdQueryHandler(
        IDocumentRepository documentRepository,
        IAttachmentRepository attachmentRepository,
        IStorageService storageService)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _attachmentRepository = attachmentRepository ?? throw new ArgumentNullException(nameof(attachmentRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    /// <inheritdoc />
    public async Task<Result<RetentionReceiptPdfResult>> Handle(GetRetentionReceiptPdfByDocIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.Documents.Document? document = await _documentRepository
            .GetByIdAsync(request.DocId, cancellationToken);

        if (document is null)
        {
            return Result.Failure<RetentionReceiptPdfResult>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        Domain.Preloads.Attachments.Attachment? activeAttachment =
            await _attachmentRepository.GetActiveAttachmentByDocumentIdAsync(request.DocId, cancellationToken);

        if (activeAttachment is null)
        {
            return Result.Failure<RetentionReceiptPdfResult>(
                Error.NotFound(
                    "Attachment.NotFound",
                    "El documento no tiene un comprobante disponible."));
        }

        try
        {
            byte[] content = await _storageService.ReadFileAsync(activeAttachment.Path, cancellationToken);
            string fileName = Path.GetFileName(activeAttachment.Path);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"RetentionReceipt-{request.DocId}.pdf";
            }

            var result = new RetentionReceiptPdfResult(content, fileName);
            return Result.Success(result);
        }
        catch (FileNotFoundException)
        {
            return Result.Failure<RetentionReceiptPdfResult>(
                Error.NotFound(
                    "Attachment.FileNotFound",
                    $"Retention receipt PDF file '{activeAttachment.Path}' was not found in storage."));
        }
    }
}
