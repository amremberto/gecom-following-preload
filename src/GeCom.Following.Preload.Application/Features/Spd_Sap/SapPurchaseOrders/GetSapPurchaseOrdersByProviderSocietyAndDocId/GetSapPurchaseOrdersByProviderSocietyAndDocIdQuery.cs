using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrdersByProviderSocietyAndDocId;

/// <summary>
/// Query to get SAP purchase orders by provider code, society code, and document number.
/// </summary>
/// <param name="ProviderCuit">The provider code to filter by.</param>
/// <param name="SocietyCuit">The society code to filter by.</param>
/// <param name="DocId">The document number to filter by.</param>
public sealed record GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery(
    string ProviderCuit,
    string SocietyCuit,
    int DocId
) : IQuery<IEnumerable<SapPurchaseOrderResponse>>;

