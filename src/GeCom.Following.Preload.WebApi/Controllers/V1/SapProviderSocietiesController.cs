using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProviderSocietiesByProviderCuit;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProvidersBySocietyCuit;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing SAP provider-society relationships.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class SapProviderSocietiesController : VersionedApiController
{
    /// <summary>
    /// Gets all societies that a provider can assign documents to.
    /// </summary>
    /// <param name="providerCuit">The provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of societies that the provider can assign documents to.</returns>
    /// <response code="200">Returns the list of societies.</response>
    /// <response code="400">If the providerCuit parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("provider/{providerCuit}/societies")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<ProviderSocietyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetProviderSocietiesByProviderCuit", "Gets all societies that a provider can assign documents to.")]
    public async Task<ActionResult<IEnumerable<ProviderSocietyResponse>>> GetSocietiesByProviderCuitAsync(
        string providerCuit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerCuit))
        {
            return Problem(
                detail: "Provider CUIT cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation.Error");
        }

        GetProviderSocietiesByProviderCuitQuery query = new(providerCuit);

        Result<IEnumerable<ProviderSocietyResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets all providers that can assign documents to a specific society.
    /// </summary>
    /// <param name="societyCuit">The society CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of providers that can assign documents to the society.</returns>
    /// <response code="200">Returns the list of providers.</response>
    /// <response code="400">If the societyCuit parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("society/{societyCuit}/providers")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<ProviderSelectItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetProvidersBySocietyCuit", "Gets all providers that can assign documents to a specific society.")]
    public async Task<ActionResult<IEnumerable<ProviderSelectItemResponse>>> GetProvidersBySocietyCuitAsync(
        string societyCuit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(societyCuit))
        {
            return Problem(
                detail: "Society CUIT cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation.Error");
        }

        GetProvidersBySocietyCuitQuery query = new(societyCuit);

        Result<IEnumerable<ProviderSelectItemResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}
