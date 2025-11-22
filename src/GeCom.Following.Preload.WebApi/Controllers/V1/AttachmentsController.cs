using Asp.Versioning;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using GeCom.Following.Preload.Application.Features.Preload.Attachments.CreateAttachment;
using GeCom.Following.Preload.Application.Features.Preload.Attachments.DeleteAttachment;
using GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAllAttachments;
using GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAttachmentById;
using GeCom.Following.Preload.Application.Features.Preload.Attachments.GetAttachmentsByDocumentId;
using GeCom.Following.Preload.Application.Features.Preload.Attachments.UpdateAttachment;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Attachments.Create;
using GeCom.Following.Preload.Contracts.Preload.Attachments.Update;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing attachments.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class AttachmentsController : VersionedApiController
{
    private readonly IStorageService _storageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentsController"/> class.
    /// </summary>
    /// <param name="storageService">The storage service.</param>
    public AttachmentsController(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    /// <summary>
    /// Gets all attachments.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all attachments.</returns>
    /// <response code="200">Returns the list of attachments.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<AttachmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllAttachments", "Gets all attachments.")]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllAttachmentsQuery query = new();

        Result<IEnumerable<AttachmentResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets an attachment by its ID.
    /// </summary>
    /// <param name="adjuntoId">Attachment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The attachment if found.</returns>
    /// <response code="200">Returns the attachment.</response>
    /// <response code="400">If the adjuntoId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the attachment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{adjuntoId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAttachmentById", "Gets an attachment by its ID.")]
    public async Task<ActionResult<AttachmentResponse>> GetByIdAsync(int adjuntoId, CancellationToken cancellationToken)
    {
        GetAttachmentByIdQuery query = new(adjuntoId);

        Result<AttachmentResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets all attachments for a specific document.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of attachments for the document.</returns>
    /// <response code="200">Returns the list of attachments.</response>
    /// <response code="400">If the docId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-document/{docId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<AttachmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAttachmentsByDocumentId", "Gets all attachments for a specific document.")]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken)
    {
        GetAttachmentsByDocumentIdQuery query = new(docId);

        Result<IEnumerable<AttachmentResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new attachment.
    /// </summary>
    /// <param name="request">The attachment data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created attachment.</returns>
    /// <response code="201">Returns the created attachment.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateAttachment", "Creates a new attachment.")]
    public async Task<ActionResult<AttachmentResponse>> CreateAsync(
        [FromBody] CreateAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        CreateAttachmentCommand command = new(
            request.Path,
            request.DocId);

        Result<AttachmentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetAllAsync));
    }

    /// <summary>
    /// Updates an existing attachment.
    /// </summary>
    /// <param name="adjuntoId">Attachment ID.</param>
    /// <param name="request">The attachment data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated attachment.</returns>
    /// <response code="200">Returns the updated attachment.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the attachment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{adjuntoId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateAttachment", "Updates an existing attachment.")]
    public async Task<ActionResult<AttachmentResponse>> UpdateAsync(
        int adjuntoId,
        [FromBody] UpdateAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        UpdateAttachmentCommand command = new(
            adjuntoId,
            request.Path);

        Result<AttachmentResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes an attachment by its ID.
    /// </summary>
    /// <param name="adjuntoId">Attachment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Attachment deleted successfully.</response>
    /// <response code="400">If the adjuntoId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the attachment was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{adjuntoId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeleteAttachment", "Deletes an attachment by its ID.")]
    public async Task<ActionResult> DeleteAsync(int adjuntoId, CancellationToken cancellationToken)
    {
        DeleteAttachmentCommand command = new(adjuntoId);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }

    /// <summary>
    /// Downloads a PDF file by attachment ID.
    /// </summary>
    /// <param name="adjuntoId">Attachment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PDF file.</returns>
    /// <response code="200">Returns the PDF file.</response>
    /// <response code="400">If the adjuntoId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the attachment was not found or file does not exist.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{adjuntoId}/download")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DownloadAttachment", "Downloads a PDF file by attachment ID.")]
    public async Task<ActionResult> DownloadAsync(int adjuntoId, CancellationToken cancellationToken)
    {
        GetAttachmentByIdQuery query = new(adjuntoId);

        Result<AttachmentResponse> result = await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            Error error = result.Error;
            return error.Type switch
            {
                ErrorType.NotFound => NotFound(error.Description),
                ErrorType.Validation => BadRequest(error.Description),
                ErrorType.Unauthorized => Unauthorized(),
                ErrorType.Forbidden => Forbid(),
                _ => StatusCode(500, error.Description)
            };
        }

        AttachmentResponse attachment = result.Value;

        // Read file from storage
        try
        {
            byte[] fileContent = await _storageService.ReadFileAsync(attachment.Path, cancellationToken);
            string fileName = Path.GetFileName(attachment.Path);

            return File(fileContent, "application/pdf", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"File not found at path: {attachment.Path}");
        }
    }
}

