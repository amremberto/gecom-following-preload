using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.States.CreateState;
using GeCom.Following.Preload.Application.Features.Preload.States.DeleteState;
using GeCom.Following.Preload.Application.Features.Preload.States.GetAllStates;
using GeCom.Following.Preload.Application.Features.Preload.States.GetStateByCode;
using GeCom.Following.Preload.Application.Features.Preload.States.GetStateById;
using GeCom.Following.Preload.Application.Features.Preload.States.UpdateState;
using GeCom.Following.Preload.Contracts.Preload.States;
using GeCom.Following.Preload.Contracts.Preload.States.Create;
using GeCom.Following.Preload.Contracts.Preload.States.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing states.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class StatesController : VersionedApiController
{
    /// <summary>
    /// Gets all states.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all states.</returns>
    /// <response code="200">Returns the list of states.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<StateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllStates", "Gets all states.")]
    public async Task<ActionResult<IEnumerable<StateResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllStatesQuery query = new();

        Result<IEnumerable<StateResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a state by its ID.
    /// </summary>
    /// <param name="estadoId">State ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The state if found.</returns>
    /// <response code="200">Returns the state.</response>
    /// <response code="400">If the estadoId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the state was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("id/{estadoId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetStateById", "Gets a state by its ID.")]
    public async Task<ActionResult<StateResponse>> GetByIdAsync(int estadoId, CancellationToken cancellationToken)
    {
        GetStateByIdQuery query = new(estadoId);

        Result<StateResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a state by its code.
    /// </summary>
    /// <param name="codigo">State code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The state if found.</returns>
    /// <response code="200">Returns the state.</response>
    /// <response code="400">If the codigo parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the state was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("code/{codigo}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetStateByCode", "Gets a state by its code.")]
    public async Task<ActionResult<StateResponse>> GetByCodeAsync(string codigo, CancellationToken cancellationToken)
    {
        GetStateByCodeQuery query = new(codigo);

        Result<StateResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new state.
    /// </summary>
    /// <param name="request">The state data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created state.</returns>
    /// <response code="201">Returns the created state.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="409">If a state with the same code already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateState", "Creates a new state.")]
    public async Task<ActionResult<StateResponse>> CreateAsync(
        [FromBody] CreateStateRequest request,
        CancellationToken cancellationToken)
    {
        CreateStateCommand command = new(
            request.Descripcion,
            request.Codigo);

        Result<StateResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetByCodeAsync), new { codigo = request.Codigo });
    }

    /// <summary>
    /// Updates an existing state.
    /// </summary>
    /// <param name="estadoId">State ID.</param>
    /// <param name="request">The state data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated state.</returns>
    /// <response code="200">Returns the updated state.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the state was not found.</response>
    /// <response code="409">If a state with the same code already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{estadoId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateState", "Updates an existing state.")]
    public async Task<ActionResult<StateResponse>> UpdateAsync(
        int estadoId,
        [FromBody] UpdateStateRequest request,
        CancellationToken cancellationToken)
    {
        UpdateStateCommand command = new(
            estadoId,
            request.Descripcion,
            request.Codigo);

        Result<StateResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a state by its ID.
    /// </summary>
    /// <param name="estadoId">State ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">State deleted successfully.</response>
    /// <response code="400">If the estadoId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the state was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{estadoId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeleteState", "Deletes a state by its ID.")]
    public async Task<ActionResult> DeleteAsync(int estadoId, CancellationToken cancellationToken)
    {
        DeleteStateCommand command = new(estadoId);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

