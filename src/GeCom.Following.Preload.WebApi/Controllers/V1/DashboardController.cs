using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for dashboard information.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class DashboardController : VersionedApiController
{
    /// <summary>
    /// Gets dashboard information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dashboard information.</returns>
    /// <response code="200">Returns the dashboard information.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken)
    {
        GetDashboardQuery query = new();

        Result<DashboardResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}

