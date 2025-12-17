namespace GeCom.Following.Preload.Contracts.Preload.Dashboard;

/// <summary>
/// Response DTO for Dashboard information.
/// </summary>
public sealed record DashboardResponse(
    int TotalProcessedDocuments,
    int TotalPurchaseOrders,
    int TotalPendingsDocuments,
    int TotalPaidDocuments
);

