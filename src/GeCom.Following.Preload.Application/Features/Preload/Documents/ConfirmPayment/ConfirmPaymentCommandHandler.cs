using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Monitor;
using GeCom.Following.Preload.Application.Abstractions.Pdf;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.ConfirmPayment;

/// <summary>
/// Handler for the ConfirmPaymentCommand. Validates document exists, is in paid state, and does not already have payment confirmed.
/// Generates Recibo de Pago PDF, creates PaymentDetail, saves file to storage, and updates document with IdDetalleDePago, IdMetodoDePago, FechaPago.
/// </summary>
internal sealed class ConfirmPaymentCommandHandler : ICommandHandler<ConfirmPaymentCommand, DocumentResponse>
{
    private const string PaidStateCode = "PagadoFin";
    private const string ConceptoPago = "Pago a Proveedores";
    private readonly IDocumentRepository _documentRepository;
    private readonly IMonitorService _monitorService;
    private readonly IPdfDocumentService _pdfDocumentService;
    private readonly IStorageService _storageService;
    private readonly IPaymentDetailRepository _paymentDetailRepository;
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPaymentCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="monitorService">The monitor service.</param>
    /// <param name="pdfDocumentService">The PDF document service.</param>
    /// <param name="storageService">The storage service.</param>
    /// <param name="paymentDetailRepository">The payment detail repository.</param>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public ConfirmPaymentCommandHandler(
        IDocumentRepository documentRepository,
        IMonitorService monitorService,
        IPdfDocumentService pdfDocumentService,
        IStorageService storageService,
        IPaymentDetailRepository paymentDetailRepository,
        IPaymentTypeRepository paymentTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
        _pdfDocumentService = pdfDocumentService ?? throw new ArgumentNullException(nameof(pdfDocumentService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _paymentDetailRepository = paymentDetailRepository ?? throw new ArgumentNullException(nameof(paymentDetailRepository));
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Obtain document
        Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);

        if (document is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        if (document.IdDetalleDePago.HasValue)
        {
            return Result.Failure<DocumentResponse>(
                Error.Conflict(
                    "Document.PaymentAlreadyConfirmed",
                    "El pago de este documento ya fue confirmado."));
        }

        if (string.IsNullOrEmpty(document.State?.Codigo) ||
            !string.Equals(document.State.Codigo, PaidStateCode, StringComparison.OrdinalIgnoreCase))
        {
            string estadoDescripcion = document.State?.Descripcion ?? document.EstadoId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "desconocido";
            return Result.Failure<DocumentResponse>(
                Error.Conflict(
                    "Document.InvalidStateForConfirmPayment",
                    $"Solo se puede confirmar pago en documentos con estado PAGADO. Estado actual: {estadoDescripcion}."));
        }

        // Obtain sapDocumentNumber (for OrdenDePago on receipt)
        var getSapDocumentNumberRequest =
            new GetSapDocumentNumberRequest(
                document.NumeroComprobante!,
                document.ProveedorCuit!,
                document.SociedadCuit!,
                document.PuntoDeVenta!,
                document.DocumentType!.Letra!);

        int? sapDocumentNumber = await _monitorService.GetSapDocumentNumberAsync(getSapDocumentNumberRequest, cancellationToken);

        // Obtain payment details number (for receipt number and file name)
        int paymentDetailNumber = await _documentRepository
            .CountAsync(d => d.IdDetalleDePago.HasValue && d.SociedadCuit == document.SociedadCuit, cancellationToken) + 1;

        // Resolve payment type by description
        PaymentType? paymentType = await _paymentTypeRepository.GetByDescripcionAsync(request.PaymentMethod, cancellationToken);
        if (paymentType is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "PaymentType.NotFound",
                    $"Tipo de pago '{request.PaymentMethod}' no encontrado."));
        }

        bool esTransferencia = paymentType.Descripcion.Contains("Transferencia", StringComparison.OrdinalIgnoreCase);
        var fechaAlta = DateOnly.FromDateTime(DateTime.Today);
        var reciboData = new ReciboDePagoData
        {
            NumeroRecibo = paymentDetailNumber.ToString("D5", System.Globalization.CultureInfo.InvariantCulture),
            FechaEmision = DateTime.UtcNow,
            ProveedorCuit = document.ProveedorCuit ?? string.Empty,
            ProveedorRazonSocial = document.Provider?.RazonSocial ?? string.Empty,
            Cliente = document.Society?.Descripcion ?? string.Empty,
            Concepto = ConceptoPago,
            ImporteRecibido = document.MontoBruto ?? 0,
            Moneda = document.Moneda,
            FechaAlta = fechaAlta.ToDateTime(TimeOnly.MinValue),
            EsTransferencia = esTransferencia,
            NroCheque = request.NumeroCheque,
            Banco = request.Banco,
            Vencimiento = request.Vencimiento,
            OrdenDePago = (sapDocumentNumber?.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };

        var pdfRequest = new PdfDocumentRequest
        {
            DocumentType = PdfDocumentType.ReciboDePago,
            ReciboDePagoData = reciboData
        };

        byte[] pdfBytes = await _pdfDocumentService.GenerateAsync(pdfRequest, cancellationToken);

        string uniqueFileName = $"Recibo_{document.DocId}_{DateTime.Today:yyyyMMdd}.pdf";
        try
        {
            await _storageService.SavePaymentDetailFileAsync(pdfBytes, uniqueFileName, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<DocumentResponse>(
                Error.Failure(
                    "Storage.UploadFailed",
                    $"No se pudo guardar el PDF en storage: {ex.Message}"));
        }

        var paymentDetail = new PaymentDetail
        {
            IdTipoDePago = paymentType.Id,
            NroCheque = request.NumeroCheque ?? string.Empty,
            Banco = request.Banco ?? string.Empty,
            Vencimiento = request.Vencimiento ?? fechaAlta,
            ImporteRecibido = document.MontoBruto ?? 0,
            FechaAlta = fechaAlta,
            NamePdf = uniqueFileName
        };

        PaymentDetail addedPaymentDetail = await _paymentDetailRepository.AddAsync(paymentDetail, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        document.IdDetalleDePago = addedPaymentDetail.Id;
        document.IdMetodoDePago = paymentType.Id;
        document.FechaPago = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        DocumentResponse response = DocumentMappings.ToResponse(document);

        return Result.Success(response);
    }
}
