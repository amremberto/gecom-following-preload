using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;

namespace GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;

/// <summary>
/// Query to get dashboard information.
/// </summary>
/// <param name="UserRoles">User roles to determine filtering strategy.</param>
/// <param name="UserEmail">User email (required for Societies role).</param>
/// <param name="ProviderCuit">Provider CUIT from claim (required for Providers role).</param>
public sealed record GetDashboardQuery(
    IReadOnlyList<string> UserRoles,
    string? UserEmail = null,
    string? ProviderCuit = null
) : IQuery<DashboardResponse>;

