using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;

namespace GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;

/// <summary>
/// Query to get dashboard information.
/// </summary>
public sealed record GetDashboardQuery : IQuery<DashboardResponse>;

