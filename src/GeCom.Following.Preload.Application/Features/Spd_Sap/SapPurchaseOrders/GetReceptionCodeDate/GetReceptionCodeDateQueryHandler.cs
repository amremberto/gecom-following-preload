using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetReceptionCodeDate;

/// <summary>
/// Handles <see cref="GetReceptionCodeDateQuery"/> requests.
/// </summary>
internal sealed class GetReceptionCodeDateQueryHandler
    : IQueryHandler<GetReceptionCodeDateQuery, GetReceptionCodeDateResponse?>
{
    private readonly ISapPurchaseOrderRepository _sapPurchaseOrderRepository;

    public GetReceptionCodeDateQueryHandler(ISapPurchaseOrderRepository sapPurchaseOrderRepository)
    {
        _sapPurchaseOrderRepository = sapPurchaseOrderRepository;
    }

    public async Task<Result<GetReceptionCodeDateResponse?>> Handle(
        GetReceptionCodeDateQuery request,
        CancellationToken cancellationToken)
    {
        GetReceptionCodeDateRequest dto = request.Request;

        if (string.IsNullOrWhiteSpace(dto.CodigoRecepcion))
        {
            return Result.Success<GetReceptionCodeDateResponse?>(null);
        }

        if (!dto.OrdenCompraId.HasValue || dto.OrdenCompraId.Value <= 0)
        {
            return Result.Success<GetReceptionCodeDateResponse?>(null);
        }

        DateTime? fecha = await _sapPurchaseOrderRepository.GetReceptionCodeDateAsync(
            dto.OrdenCompraId.Value,
            dto.CodigoRecepcion,
            cancellationToken);

        if (fecha is null)
        {
            return Result.Success<GetReceptionCodeDateResponse?>(null);
        }

        GetReceptionCodeDateResponse response = new(
            dto.OrdenCompraId,
            dto.CodigoRecepcion,
            fecha.Value);

        return Result.Success<GetReceptionCodeDateResponse?>(response);
    }
}

