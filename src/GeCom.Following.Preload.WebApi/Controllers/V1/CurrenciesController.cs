using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.CreateCurrency;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.DeleteCurrency;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrencies;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrenciesPaged;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyByCode;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyById;
using GeCom.Following.Preload.Application.Features.Preload.Currencies.UpdateCurrency;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Contracts.Preload.Currencies.Create;
using GeCom.Following.Preload.Contracts.Preload.Currencies.GetAll;
using GeCom.Following.Preload.Contracts.Preload.Currencies.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing currencies.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class CurrenciesController : VersionedApiController
{
    /// <summary>
    /// Gets all currencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all currencies.</returns>
    /// <response code="200">Returns the list of currencies.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CurrencyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        GetAllCurrenciesQuery query = new();

        Result<IEnumerable<CurrencyResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets currencies with pagination.
    /// </summary>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result with currencies.</returns>
    /// <response code="200">Returns paged currencies.</response>
    /// <response code="400">If pagination parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<CurrencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<CurrencyResponse>>> GetAllPaged([FromQuery] GetAllCurrenciesRequest request, CancellationToken cancellationToken)
    {
        int page = request.Page ?? 1;
        int pageSize = request.PageSize ?? 20;

        if (page <= 0 || pageSize <= 0)
        {
            return Problem(
                detail: "Invalid pagination parameters.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Pagination.Invalid");
        }

        GetAllCurrenciesPagedQuery query = new(page, pageSize);

        Result<PagedResponse<CurrencyResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a currency by its ID.
    /// </summary>
    /// <param name="id">Currency ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The currency if found.</returns>
    /// <response code="200">Returns the currency.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the currency was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrencyResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        GetCurrencyByIdQuery query = new(id);

        Result<CurrencyResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a currency by its code.
    /// </summary>
    /// <param name="codigo">Currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The currency if found.</returns>
    /// <response code="200">Returns the currency.</response>
    /// <response code="400">If the codigo parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the currency was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrencyResponse>> GetByCodigo(string codigo, CancellationToken cancellationToken)
    {
        GetCurrencyByCodeQuery query = new(codigo);

        Result<CurrencyResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new currency.
    /// </summary>
    /// <param name="request">The currency data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created currency.</returns>
    /// <response code="201">Returns the created currency.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="409">If a currency with the same code already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrencyResponse>> Create(
        [FromBody] CreateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        CreateCurrencyCommand command = new(
            request.Codigo,
            request.Descripcion,
            request.CodigoAfip);

        Result<CurrencyResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetByCodigo), new { codigo = request.Codigo });
    }

    /// <summary>
    /// Updates an existing currency.
    /// </summary>
    /// <param name="request">The currency data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated currency.</returns>
    /// <response code="200">Returns the updated currency.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the currency was not found.</response>
    /// <response code="409">If a currency with the same code already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrencyResponse>> Update(
        [FromBody] UpdateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        UpdateCurrencyCommand command = new(
            request.MonedaId,
            request.Codigo,
            request.Descripcion,
            request.CodigoAfip);

        Result<CurrencyResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a currency by its ID.
    /// </summary>
    /// <param name="id">Currency ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Currency deleted successfully.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the currency was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        DeleteCurrencyCommand command = new(id);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

