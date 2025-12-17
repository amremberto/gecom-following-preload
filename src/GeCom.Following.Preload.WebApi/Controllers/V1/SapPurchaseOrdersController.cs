using System.Security.Claims;
using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrders;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing SAP purchase orders.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class SapPurchaseOrdersController : VersionedApiController
{
    /// <summary>
    /// Gets SAP purchase orders based on user role.
    /// - Providers: Returns purchase orders for the provider's account number (obtained from CUIT).
    /// - Societies: Returns purchase orders for all societies assigned to the user (filtered by Sociedadfi).
    /// - Administrator/ReadOnly: Returns all purchase orders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of SAP purchase orders based on user role.</returns>
    /// <response code="200">Returns the list of SAP purchase orders.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<SapPurchaseOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetSapPurchaseOrders", "Gets SAP purchase orders based on user role.")]
    public async Task<ActionResult<IEnumerable<SapPurchaseOrderResponse>>> GetAsync(CancellationToken cancellationToken)
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

        GetSapPurchaseOrdersQuery query = new(
            userRoles,
            userEmail,
            providerCuit);

        Result<IEnumerable<SapPurchaseOrderResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}
