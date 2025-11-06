using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocieties;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.SharedKernel.Results;

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

        Result<IEnumerable<SocietyResponse>>? result =
            await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return Problem(
                detail: result.Error.Description,
                statusCode: StatusCodes.Status500InternalServerError,
                title: result.Error.Code);
        }

        return Ok(result.Value);
    }
}

