using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetAllDocuments;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing documents.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class DocumentsController : VersionedApiController
{
    /// <summary>
    /// Gets all documents.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all documents.</returns>
    /// <response code="200">Returns the list of documents.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        GetAllDocumentsQuery query = new();

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}

