using System.Security.Claims;
using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.LinkSapPurchaseOrderToDocument;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.UnlinkSapPurchaseOrderFromDocument;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrders;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrdersByProviderSocietyAndDocId;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetReceptionCodeDate;
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

    /// <summary>
    /// Gets SAP purchase orders by provider code, society code, and document number.
    /// </summary>
    /// <param name="providerCuit">The provider code to filter by.</param>
    /// <param name="societyCuit">The society code to filter by.</param>
    /// <param name="docId">The document number to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of SAP purchase orders matching the specified criteria.</returns>
    /// <response code="200">Returns the list of SAP purchase orders.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-provider-society-doc")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<SapPurchaseOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetSapPurchaseOrdersByProviderSocietyAndDocId", "Gets SAP purchase orders by provider code, society code, and document number.")]
    public async Task<ActionResult<IEnumerable<SapPurchaseOrderResponse>>> GetByProviderSocietyAndDocIdAsync(
        [FromQuery] string providerCuit,
        [FromQuery] string societyCuit,
        [FromQuery] int docId,
        CancellationToken cancellationToken)
    {
        GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery query = new(
            providerCuit,
            societyCuit,
            docId);

        Result<IEnumerable<SapPurchaseOrderResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets the reception code date for a SAP purchase order, based on
    /// the order primary key and reception code.
    /// </summary>
    /// <param name="request">Request payload containing order ID and reception code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reception code date information.</returns>
    /// <response code="200">Returns the reception code date information.</response>
    /// <response code="404">If no purchase order is found for the given data.</response>
    [HttpPost("reception-code-date")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(GetReceptionCodeDateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [OpenApiOperation("GetReceptionCodeDate", "Gets the reception code date for a SAP purchase order.")]
    public async Task<ActionResult<GetReceptionCodeDateResponse?>> GetReceptionCodeDateAsync(
        [FromBody] GetReceptionCodeDateRequest request,
        CancellationToken cancellationToken)
    {
        GetReceptionCodeDateQuery query = new(request);

        Result<GetReceptionCodeDateResponse?> result =
            await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Match(this);
        }

        GetReceptionCodeDateResponse? value = result.Value;

        if (value is null)
        {
            return NotFound();
        }

        return Ok(value);
    }

    /// <summary>
    /// Links a SAP purchase order to a preload document.
    /// Compatible with the legacy action semantics of <c>api/oc/link-document</c>.
    /// </summary>
    /// <param name="request">Request payload containing document and purchase order link data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created link data.</returns>
    /// <response code="201">Returns the linked purchase order data.</response>
    /// <response code="400">If request validation fails.</response>
    /// <response code="404">If the target document does not exist.</response>
    /// <response code="409">If an active link already exists.</response>
    [HttpPost("link-document")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(LinkSapPurchaseOrderToDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [OpenApiOperation("LinkSapPurchaseOrderToDocument", "Links a SAP purchase order to a preload document.")]
    public async Task<ActionResult<LinkSapPurchaseOrderToDocumentResponse>> LinkToDocumentAsync(
        [FromBody] LinkSapPurchaseOrderToDocumentRequest request,
        CancellationToken cancellationToken)
    {
        LinkSapPurchaseOrderToDocumentCommand command = new(
            request.Ocid,
            request.CodigoRecepcion,
            request.CantidadAFacturar,
            request.DocId,
            request.OrdenCompraId,
            request.NroOc,
            request.PosicionOc,
            request.CodigoSociedadFi,
            request.ProveedorSap,
            request.ImporteNetoAnticipo);

        Result<LinkSapPurchaseOrderToDocumentResponse> result =
            await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            // 201 sin CreatedAtAction: evita "No route matches the supplied values" con api versioning.
            ObjectResult objectResult = new(result.Value)
            {
                StatusCode = StatusCodes.Status201Created
            };
            return objectResult;
        }

        return CustomResults.ProblemActionResult(this, result);
    }

    /// <summary>
    /// Unlinks a SAP purchase order from a preload document.
    /// Compatible with the legacy action semantics of <c>api/oc/unlink-document</c>.
    /// </summary>
    /// <param name="request">Request payload containing legacy unlink key data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content when the link was successfully removed.</returns>
    /// <response code="204">If the link was successfully unlinked.</response>
    /// <response code="400">If request validation fails.</response>
    /// <response code="404">If an active link was not found for provided data.</response>
    [HttpPost("unlink-document")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [OpenApiOperation("UnlinkSapPurchaseOrderFromDocument", "Unlinks a SAP purchase order from a preload document.")]
    public async Task<ActionResult> UnlinkFromDocumentAsync(
        [FromBody] UnlinkSapPurchaseOrderFromDocumentRequest request,
        CancellationToken cancellationToken)
    {
        UnlinkSapPurchaseOrderFromDocumentCommand command = new(
            request.DocId,
            request.Posicion,
            request.NumeroDocumento,
            request.CodigoRecepcion);

        Result result = await Mediator.Send(command, cancellationToken);
        return result.MatchDeleted(this);
    }
}
