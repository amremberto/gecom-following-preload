using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrders;

/// <summary>
/// Query to get SAP purchase orders based on user role.
/// - Providers: Returns purchase orders for the provider's account number (obtained from CUIT).
/// - Societies: Returns purchase orders for all societies assigned to the user (filtered by Sociedadfi).
/// - Administrator/ReadOnly: Returns all purchase orders.
/// </summary>
/// <param name="UserRoles">User roles to determine filtering strategy. Must contain at least one role.</param>
/// <param name="UserEmail">User email (required for Societies role).</param>
/// <param name="ProviderCuit">Provider CUIT from claim (required for Providers role).</param>
public sealed record GetSapPurchaseOrdersQuery(
    IReadOnlyList<string> UserRoles,
    string? UserEmail,
    string? ProviderCuit
) : IQuery<IEnumerable<SapPurchaseOrderResponse>>;
