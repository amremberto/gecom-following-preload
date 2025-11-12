using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetAllDocuments;
using GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDatesAndProvider;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

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
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllDocuments", "Gets all documents.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllDocumentsQuery query = new();

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets documents by emission date range and optionally by provider CUIT.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="providerCuit">Provider CUIT (optional). If not provided, returns documents from all providers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of documents matching the criteria.</returns>
    /// <response code="200">Returns the list of documents.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-dates-and-provider")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDocumentsByDatesAndProvider", "Gets documents by emission date range and optionally by provider CUIT.")]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetByDatesAndProviderAsync(
        [FromQuery] DateOnly dateFrom,
        [FromQuery] DateOnly dateTo,
        [FromQuery] string? providerCuit,
        CancellationToken cancellationToken)
    {
        GetDocumentsByEmissionDatesAndProviderQuery query = new(
            dateFrom,
            dateTo,
            providerCuit);

        Result<IEnumerable<DocumentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }
}

