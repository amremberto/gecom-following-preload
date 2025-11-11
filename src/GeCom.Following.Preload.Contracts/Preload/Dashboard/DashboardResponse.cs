namespace GeCom.Following.Preload.Contracts.Preload.Dashboard;

/// <summary>
/// Response DTO for Dashboard information.
/// </summary>
public sealed record DashboardResponse(
    int TotalDocuments,
    int TotalPurchaseOrders,
    int TotalPendingsDocuments
);

