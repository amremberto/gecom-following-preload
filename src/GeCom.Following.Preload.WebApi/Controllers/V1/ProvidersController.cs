using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Providers.GetAllProviders;
using GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderByCuit;
using GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderById;
using GeCom.Following.Preload.Application.Features.Preload.Providers.SearchProviders;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing providers.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class ProvidersController : VersionedApiController
{
    /// <summary>
    /// Gets all providers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all providers.</returns>
    /// <response code="200">Returns the list of providers.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProviderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProviderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        GetAllProvidersQuery query = new();

        Result<IEnumerable<ProviderResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a provider by its ID.
    /// </summary>
    /// <param name="id">Provider ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider if found.</returns>
    /// <response code="200">Returns the provider.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the provider was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProviderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        GetProviderByIdQuery query = new(id);

        Result<ProviderResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a provider by its CUIT.
    /// </summary>
    /// <param name="cuit">Provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider if found.</returns>
    /// <response code="200">Returns the provider.</response>
    /// <response code="400">If the cuit parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the provider was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("cuit/{cuit}")]
    [ProducesResponseType(typeof(ProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProviderResponse>> GetByCuit(string cuit, CancellationToken cancellationToken)
    {
        GetProviderByCuitQuery query = new(cuit);

        Result<ProviderResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Searches for providers by search text.
    /// </summary>
    /// <param name="searchText">Search text to match against provider CUIT, business name, or email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of providers matching the search criteria.</returns>
    /// <response code="200">Returns the list of matching providers.</response>
    /// <response code="400">If the search text parameter is invalid or empty.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProviderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProviderResponse>>> Search(
        [FromQuery] string searchText,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return BadRequest("Search text cannot be null or empty.");
        }

        SearchProvidersQuery query = new(searchText);

        Result<IEnumerable<ProviderResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}

