using System.Security.Claims;
using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Societies.CreateSociety;
using GeCom.Following.Preload.Application.Features.Preload.Societies.DeleteSociety;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocieties;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocietiesPaged;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietiesByUserEmail;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietiesForCurrentUser;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCodigo;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCuit;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyById;
using GeCom.Following.Preload.Application.Features.Preload.Societies.UpdateSociety;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Contracts.Preload.Societies.Create;
using GeCom.Following.Preload.Contracts.Preload.Societies.GetAll;
using GeCom.Following.Preload.Contracts.Preload.Societies.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing societies.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class SocietiesController : VersionedApiController
{
    /// <summary>
    /// Gets all societies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all societies.</returns>
    /// <response code="200">Returns the list of societies.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SocietyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SocietyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        GetAllSocietiesQuery query = new();

        Result<IEnumerable<SocietyResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets societies with pagination.
    /// </summary>
    /// <param name="request">Pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result with societies.</returns>
    /// <response code="200">Returns paged societies.</response>
    /// <response code="400">If pagination parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<SocietyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<SocietyResponse>>> GetAllPaged([FromQuery] GetAllSocietiesRequest request, CancellationToken cancellationToken)
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

        GetAllSocietiesPagedQuery query = new(page, pageSize);

        Result<PagedResponse<SocietyResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a society by its ID.
    /// </summary>
    /// <param name="id">Society ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The society if found.</returns>
    /// <response code="200">Returns the society.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the society was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(SocietyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SocietyResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        GetSocietyByIdQuery query = new(id);

        Result<SocietyResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a society by its code.
    /// </summary>
    /// <param name="codigo">Society code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The society if found.</returns>
    /// <response code="200">Returns the society.</response>
    /// <response code="400">If the codigo parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the society was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(SocietyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SocietyResponse>> GetByCodigo(string codigo, CancellationToken cancellationToken)
    {
        GetSocietyByCodigoQuery query = new(codigo);

        Result<SocietyResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a society by its CUIT.
    /// </summary>
    /// <param name="cuit">Society CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The society if found.</returns>
    /// <response code="200">Returns the society.</response>
    /// <response code="400">If the cuit parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the society was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("cuit/{cuit}")]
    [ProducesResponseType(typeof(SocietyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SocietyResponse>> GetByCuit(string cuit, CancellationToken cancellationToken)
    {
        GetSocietyByCuitQuery query = new(cuit);

        Result<SocietyResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new society.
    /// </summary>
    /// <param name="request">The society data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created society.</returns>
    /// <response code="201">Returns the created society.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="409">If a society with the same code or CUIT already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(SocietyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SocietyResponse>> Create(
        [FromBody] CreateSocietyRequest request,
        CancellationToken cancellationToken)
    {
        CreateSocietyCommand command = new(
            request.Codigo,
            request.Descripcion,
            request.Cuit,
            request.EsPrecarga);

        Result<SocietyResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetByCodigo), new { codigo = request.Codigo });
    }

    /// <summary>
    /// Updates an existing society.
    /// </summary>
    /// <param name="request">The society data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated society.</returns>
    /// <response code="200">Returns the updated society.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the society was not found.</response>
    /// <response code="409">If a society with the same code or CUIT already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(SocietyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SocietyResponse>> Update(
        [FromBody] UpdateSocietyRequest request,
        CancellationToken cancellationToken)
    {
        UpdateSocietyCommand command = new(
            request.SocId,
            request.Codigo,
            request.Descripcion,
            request.Cuit,
            request.EsPrecarga);

        Result<SocietyResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Delete a society by its ID.
    /// </summary>
    /// <param name="id">Society ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Society deleted successfully.</response>
    /// <response code="400">If the id parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the society was not found.</response>
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
        DeleteSocietyCommand command = new(id);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }

    /// <summary>
    /// Gets societies for the current user based on their role.
    /// The API automatically determines which societies to return based on the authenticated user's role:
    /// - Provider: Returns societies that the provider can assign documents to
    /// - Society: Returns societies that the user has access to
    /// - Administrator/ReadOnly: Returns empty result (societies are not needed for these roles in edit context)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of societies available for the current user based on their role.</returns>
    /// <response code="200">Returns the list of societies.</response>
    /// <response code="400">If the user roles or required claims are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("for-current-user")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<SocietySelectItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SocietySelectItemResponse>>> GetForCurrentUser(CancellationToken cancellationToken)
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
            return Problem(
                detail: "User roles not found in the authentication token.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation.Error");
        }

        // Get user email (for Society role)
        string? userEmail = User.FindFirst("email")?.Value ??
                           User.FindFirst(ClaimTypes.Email)?.Value;

        // Get provider CUIT from claim (for Provider role)
        string? providerCuit = User.FindFirst(AuthorizationConstants.SocietyCuitClaimType)?.Value;

        GetSocietiesForCurrentUserQuery query = new(
            userRoles,
            userEmail,
            providerCuit);

        Result<IEnumerable<SocietySelectItemResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets societies by user email for select dropdowns.
    /// </summary>
    /// <param name="userEmail">User email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of societies (CUIT and description only) available for the user.</returns>
    /// <response code="200">Returns the list of societies.</response>
    /// <response code="400">If the userEmail parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-user-email/{userEmail}")]
    [ProducesResponseType(typeof(IEnumerable<SocietySelectItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SocietySelectItemResponse>>> GetByUserEmail(string userEmail, CancellationToken cancellationToken)
    {
        GetSocietiesByUserEmailQuery query = new(userEmail);

        Result<IEnumerable<SocietySelectItemResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}
