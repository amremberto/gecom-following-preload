using GeCom.Following.Preload.Contracts.Preload.Dashboard;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for dashboard-related operations.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets dashboard information from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dashboard response data.</returns>
    Task<DashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default);
}

