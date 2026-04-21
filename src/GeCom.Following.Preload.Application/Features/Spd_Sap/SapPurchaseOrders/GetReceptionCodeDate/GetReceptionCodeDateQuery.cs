using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetReceptionCodeDate;

/// <summary>
/// Query to retrieve the reception code date for a SAP purchase order.
/// </summary>
public sealed record GetReceptionCodeDateQuery(GetReceptionCodeDateRequest Request)
    : IQuery<GetReceptionCodeDateResponse?>;

