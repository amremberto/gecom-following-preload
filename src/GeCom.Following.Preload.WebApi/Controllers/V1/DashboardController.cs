using System.Security.Claims;
using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

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
    /// Gets dashboard information based on user role.
    /// - Providers: Returns statistics for documents associated with the provider's CUIT.
    /// - Societies: Returns statistics for documents from all societies assigned to the user.
    /// - Administrator/ReadOnly: Returns statistics for all documents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dashboard information filtered by user role.</returns>
    /// <response code="200">Returns the dashboard information.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDashboard data", "Gets dashboard information based on user role.")]
    public async Task<ActionResult<DashboardResponse>> GetAsync(CancellationToken cancellationToken)
    {
        // Get user roles
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role ||
                        c.Type == AuthorizationConstants.RoleClaimType ||
                        c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        if (userRoles.Count == 0)
        {
            return BadRequest("User roles not found in the authentication token.");
        }

        // Get user email (for Societies role)
        string? userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(ClaimTypes.Email)?.Value;

        // Validate email is present for Societies role
        if (string.IsNullOrWhiteSpace(userEmail) &&
            userRoles.Contains("Following.Preload.Societies", StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("User email is required for users with Societies role, but email claim was not found in the authentication token.");
        }

        // Get provider CUIT from claim (for Providers role)
        string? providerCuit = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType)?.Value;

        GetDashboardQuery query = new(
            userRoles,
            userEmail,
            providerCuit);

        Result<DashboardResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}

