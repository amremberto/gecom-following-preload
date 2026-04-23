using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.UnlinkSapPurchaseOrderFromDocument;

/// <summary>
/// Handler for UnlinkSapPurchaseOrderFromDocumentCommand.
/// </summary>
internal sealed class UnlinkSapPurchaseOrderFromDocumentCommandHandler : ICommandHandler<UnlinkSapPurchaseOrderFromDocumentCommand>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnlinkSapPurchaseOrderFromDocumentCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UnlinkSapPurchaseOrderFromDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        PurchaseOrder? activeLink = await _purchaseOrderRepository.GetActiveLinkForUnlinkAsync(
            request.DocId,
            request.NumeroDocumento,
            request.Posicion,
            request.CodigoRecepcion,
            cancellationToken);

        if (activeLink is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "PurchaseOrder.LinkNotFound",
                    "An active purchase order link was not found for the provided document and purchase order data."));
        }

        activeLink.FechaBaja = DateTime.UtcNow;

        await _purchaseOrderRepository.UpdateAsync(activeLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
