using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.CreateUserSocietyAssignment;
using GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.DeleteUserSocietyAssignment;
using GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetAllUserSocietyAssignments;
using GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetAllUserSocietyAssignmentsPaged;
using GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetUserSocietyAssignmentById;
using GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.UpdateUserSocietyAssignment;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments.Create;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments.GetAll;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing user society assignments.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class UserSocietyAssignmentsController : VersionedApiController
{
    /// <summary>
    /// Gets all user society assignments.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all user society assignments.</returns>
    /// <response code="200">Returns the list of user society assignments.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserSocietyAssignmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllUserSocietyAssignments", "Gets all user society assignments.")]
    public async Task<ActionResult<IEnumerable<UserSocietyAssignmentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        GetAllUserSocietyAssignmentsQuery query = new();

        Result<IEnumerable<UserSocietyAssignmentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets user society assignments with pagination.
    /// </summary>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result with user society assignments.</returns>
    /// <response code="200">Returns paged user society assignments.</response>
    /// <response code="400">If pagination parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<UserSocietyAssignmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllUserSocietyAssignmentsPaged", "Gets user society assignments with pagination.")]
    public async Task<ActionResult<PagedResponse<UserSocietyAssignmentResponse>>> GetAllPaged(
        [FromQuery] GetAllUserSocietyAssignmentsRequest request,
        CancellationToken cancellationToken)
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

        GetAllUserSocietyAssignmentsPagedQuery query = new(page, pageSize);

        Result<PagedResponse<UserSocietyAssignmentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a user society assignment by its ID.
    /// </summary>
    /// <param name="id">User society assignment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user society assignment if found.</returns>
    /// <response code="200">Returns the user society assignment.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the user society assignment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserSocietyAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetUserSocietyAssignmentById", "Gets a user society assignment by its ID.")]
    public async Task<ActionResult<UserSocietyAssignmentResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        GetUserSocietyAssignmentByIdQuery query = new(id);

        Result<UserSocietyAssignmentResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new user society assignment.
    /// </summary>
    /// <param name="request">The user society assignment data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user society assignment.</returns>
    /// <response code="201">Returns the created user society assignment.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserSocietyAssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateUserSocietyAssignment", "Creates a new user society assignment.")]
    public async Task<ActionResult<UserSocietyAssignmentResponse>> Create(
        [FromBody] CreateUserSocietyAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        CreateUserSocietyAssignmentCommand command = new(
            request.Email,
            request.CuitClient,
            request.SociedadFi);

        Result<UserSocietyAssignmentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetById), new { id = result.IsSuccess ? result.Value!.Id : 0 });
    }

    /// <summary>
    /// Updates an existing user society assignment.
    /// </summary>
    /// <param name="request">The user society assignment data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user society assignment.</returns>
    /// <response code="200">Returns the updated user society assignment.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the user society assignment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut]
    [ProducesResponseType(typeof(UserSocietyAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateUserSocietyAssignment", "Updates an existing user society assignment.")]
    public async Task<ActionResult<UserSocietyAssignmentResponse>> Update(
        [FromBody] UpdateUserSocietyAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        UpdateUserSocietyAssignmentCommand command = new(
            request.Id,
            request.Email,
            request.CuitClient,
            request.SociedadFi);

        Result<UserSocietyAssignmentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a user society assignment by its ID.
    /// </summary>
    /// <param name="id">User society assignment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">User society assignment deleted successfully.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the user society assignment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeleteUserSocietyAssignment", "Deletes a user society assignment by its ID.")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        DeleteUserSocietyAssignmentCommand command = new(id);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

