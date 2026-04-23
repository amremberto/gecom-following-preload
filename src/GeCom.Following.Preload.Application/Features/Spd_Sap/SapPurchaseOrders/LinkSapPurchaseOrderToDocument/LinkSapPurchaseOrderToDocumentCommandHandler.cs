using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.LinkSapPurchaseOrderToDocument;

/// <summary>
/// Handler for LinkSapPurchaseOrderToDocumentCommand.
/// </summary>
internal sealed class LinkSapPurchaseOrderToDocumentCommandHandler
    : ICommandHandler<LinkSapPurchaseOrderToDocumentCommand, LinkSapPurchaseOrderToDocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LinkSapPurchaseOrderToDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<LinkSapPurchaseOrderToDocumentResponse>> Handle(
        LinkSapPurchaseOrderToDocumentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (document is null)
        {
            return Result.Failure<LinkSapPurchaseOrderToDocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        bool hasActiveLink = await _purchaseOrderRepository.ExistsActiveLinkAsync(
            request.DocId,
            request.OrdenCompraId,
            request.NroOc,
            request.PosicionOc,
            request.CodigoSociedadFi,
            request.ProveedorSap,
            cancellationToken);

        if (hasActiveLink)
        {
            return Result.Failure<LinkSapPurchaseOrderToDocumentResponse>(
                Error.Conflict(
                    "PurchaseOrder.LinkAlreadyExists",
                    "An active link already exists for the same document and purchase order position."));
        }

        decimal quantityToInvoice = request.CantidadAFacturar ?? request.ImporteNetoAnticipo ?? 0m;

        PurchaseOrder purchaseOrder = new()
        {
            CodigoRecepcion = string.IsNullOrWhiteSpace(request.CodigoRecepcion)
                ? null
                : request.CodigoRecepcion.Trim(),
            CantidadAfacturar = quantityToInvoice,
            DocId = request.DocId,
            OrdenCompraId = request.OrdenCompraId,
            FechaCreacion = DateTime.UtcNow,
            NroOc = request.NroOc.Trim(),
            PosicionOc = request.PosicionOc,
            CodigoSociedadFi = request.CodigoSociedadFi.Trim(),
            ProveedorSap = request.ProveedorSap.Trim()
        };

        PurchaseOrder addedPurchaseOrder = await _purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        LinkSapPurchaseOrderToDocumentResponse response = new(
            addedPurchaseOrder.Ocid,
            addedPurchaseOrder.DocId,
            addedPurchaseOrder.OrdenCompraId,
            addedPurchaseOrder.NroOc,
            addedPurchaseOrder.PosicionOc,
            addedPurchaseOrder.CodigoSociedadFi,
            addedPurchaseOrder.ProveedorSap,
            addedPurchaseOrder.CodigoRecepcion,
            addedPurchaseOrder.CantidadAfacturar,
            addedPurchaseOrder.FechaCreacion);

        return Result.Success(response);
    }
}
