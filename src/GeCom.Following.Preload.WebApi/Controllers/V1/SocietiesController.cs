using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocieties;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocietiesPaged;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Contracts.Preload.Societies.GetAll;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Results;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing societies.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
public sealed class SocietiesController : VersionedApiController
{
    /// <summary>
    /// Gets all societies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all societies.</returns>
    /// <response code="200">Returns the list of societies.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SocietyResponse>), StatusCodes.Status200OK)]
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
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<SocietyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
}

