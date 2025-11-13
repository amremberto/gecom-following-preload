using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.CreateDocumentType;
using GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.DeleteDocumentType;
using GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetAllDocumentTypes;
using GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetDocumentTypeByCode;
using GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetDocumentTypeById;
using GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.UpdateDocumentType;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes.Create;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing document types.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class DocumentTypesController : VersionedApiController
{
    /// <summary>
    /// Gets all document types.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all document types.</returns>
    /// <response code="200">Returns the list of document types.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllDocumentTypes", "Gets all document types.")]
    public async Task<ActionResult<IEnumerable<DocumentTypeResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllDocumentTypesQuery query = new();

        Result<IEnumerable<DocumentTypeResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a document type by its ID.
    /// </summary>
    /// <param name="tipoDocId">Document type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document type if found.</returns>
    /// <response code="200">Returns the document type.</response>
    /// <response code="400">If the tipoDocId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document type was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("id/{tipoDocId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDocumentTypeById", "Gets a document type by its ID.")]
    public async Task<ActionResult<DocumentTypeResponse>> GetByIdAsync(int tipoDocId, CancellationToken cancellationToken)
    {
        GetDocumentTypeByIdQuery query = new(tipoDocId);

        Result<DocumentTypeResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a document type by its code.
    /// </summary>
    /// <param name="codigo">Document type code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document type if found.</returns>
    /// <response code="200">Returns the document type.</response>
    /// <response code="400">If the codigo parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document type was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("code/{codigo}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetDocumentTypeByCode", "Gets a document type by its code.")]
    public async Task<ActionResult<DocumentTypeResponse>> GetByCodeAsync(string codigo, CancellationToken cancellationToken)
    {
        GetDocumentTypeByCodeQuery query = new(codigo);

        Result<DocumentTypeResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new document type.
    /// </summary>
    /// <param name="request">The document type data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created document type.</returns>
    /// <response code="201">Returns the created document type.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="409">If a document type with the same code already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateDocumentType", "Creates a new document type.")]
    public async Task<ActionResult<DocumentTypeResponse>> CreateAsync(
        [FromBody] CreateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreateDocumentTypeCommand command = new(
            request.Descripcion,
            request.Letra,
            request.Codigo,
            request.DescripcionLarga,
            request.IsFec);

        Result<DocumentTypeResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetByCodeAsync), new { codigo = request.Codigo });
    }

    /// <summary>
    /// Updates an existing document type.
    /// </summary>
    /// <param name="tipoDocId">Document type ID.</param>
    /// <param name="request">The document type data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document type.</returns>
    /// <response code="200">Returns the updated document type.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document type was not found.</response>
    /// <response code="409">If a document type with the same code already exists.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{tipoDocId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(typeof(DocumentTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateDocumentType", "Updates an existing document type.")]
    public async Task<ActionResult<DocumentTypeResponse>> UpdateAsync(
        int tipoDocId,
        [FromBody] UpdateDocumentTypeRequest request,
        CancellationToken cancellationToken)
    {
        UpdateDocumentTypeCommand command = new(
            tipoDocId,
            request.Descripcion,
            request.Letra,
            request.Codigo,
            request.DescripcionLarga,
            request.IsFec);

        Result<DocumentTypeResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a document type by its ID.
    /// </summary>
    /// <param name="tipoDocId">Document type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Document type deleted successfully.</response>
    /// <response code="400">If the tipoDocId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document type was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{tipoDocId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequireAdministrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeleteDocumentType", "Deletes a document type by its ID.")]
    public async Task<ActionResult> DeleteAsync(int tipoDocId, CancellationToken cancellationToken)
    {
        DeleteDocumentTypeCommand command = new(tipoDocId);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

