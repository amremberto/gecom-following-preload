using Asp.Versioning;
using GeCom.Following.Preload.Application.Features.Preload.Notes.CreateNote;
using GeCom.Following.Preload.Application.Features.Preload.Notes.DeleteNote;
using GeCom.Following.Preload.Application.Features.Preload.Notes.GetAllNotes;
using GeCom.Following.Preload.Application.Features.Preload.Notes.GetNoteById;
using GeCom.Following.Preload.Application.Features.Preload.Notes.GetNotesByDocumentId;
using GeCom.Following.Preload.Application.Features.Preload.Notes.UpdateNote;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Contracts.Preload.Notes.Create;
using GeCom.Following.Preload.Contracts.Preload.Notes.Update;
using GeCom.Following.Preload.SharedKernel.Results;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.Results;
using Microsoft.AspNetCore.Authorization;
using NSwag.Annotations;

namespace GeCom.Following.Preload.WebApi.Controllers.V1;

/// <summary>
/// Controller for managing notes.
/// </summary>
[ApiVersion("1.0")]
[Produces("application/json")]
[Authorize] // Require authentication by default for all endpoints
public sealed class NotesController : VersionedApiController
{
    /// <summary>
    /// Gets all notes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all notes.</returns>
    /// <response code="200">Returns the list of notes.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<NoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetAllNotes", "Gets all notes.")]
    public async Task<ActionResult<IEnumerable<NoteResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        GetAllNotesQuery query = new();

        Result<IEnumerable<NoteResponse>> result =
            await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets a note by its ID.
    /// </summary>
    /// <param name="notaId">Note ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The note if found.</returns>
    /// <response code="200">Returns the note.</response>
    /// <response code="400">If the notaId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the note was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("{notaId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetNoteById", "Gets a note by its ID.")]
    public async Task<ActionResult<NoteResponse>> GetByIdAsync(int notaId, CancellationToken cancellationToken)
    {
        GetNoteByIdQuery query = new(notaId);

        Result<NoteResponse> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Gets all notes for a specific document.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of notes for the document.</returns>
    /// <response code="200">Returns the list of notes.</response>
    /// <response code="400">If the docId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpGet("by-document/{docId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadRead)]
    [ProducesResponseType(typeof(IEnumerable<NoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("GetNotesByDocumentId", "Gets all notes for a specific document.")]
    public async Task<ActionResult<IEnumerable<NoteResponse>>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken)
    {
        GetNotesByDocumentIdQuery query = new(docId);

        Result<IEnumerable<NoteResponse>> result = await Mediator.Send(query, cancellationToken);

        return result.Match(this);
    }

    /// <summary>
    /// Creates a new note.
    /// </summary>
    /// <param name="request">The note data to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created note.</returns>
    /// <response code="201">Returns the created note.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the document was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("CreateNote", "Creates a new note.")]
    public async Task<ActionResult<NoteResponse>> CreateAsync(
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        CreateNoteCommand command = new(
            request.DocId,
            request.Descripcion,
            request.UsuarioCreacion);

        Result<NoteResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchCreated(this, nameof(GetAllAsync));
    }

    /// <summary>
    /// Updates an existing note.
    /// </summary>
    /// <param name="notaId">Note ID.</param>
    /// <param name="request">The note data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated note.</returns>
    /// <response code="200">Returns the updated note.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the note was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPut("{notaId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("UpdateNote", "Updates an existing note.")]
    public async Task<ActionResult<NoteResponse>> UpdateAsync(
        int notaId,
        [FromBody] UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        UpdateNoteCommand command = new(
            notaId,
            request.Descripcion);

        Result<NoteResponse> result = await Mediator.Send(command, cancellationToken);

        return result.MatchUpdated(this);
    }

    /// <summary>
    /// Deletes a note by its ID.
    /// </summary>
    /// <param name="notaId">Note ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Note deleted successfully.</response>
    /// <response code="400">If the notaId parameter is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the required permissions.</response>
    /// <response code="404">If the note was not found.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpDelete("{notaId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.RequirePreloadWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation("DeleteNote", "Deletes a note by its ID.")]
    public async Task<ActionResult> DeleteAsync(int notaId, CancellationToken cancellationToken)
    {
        DeleteNoteCommand command = new(notaId);

        Result result = await Mediator.Send(command, cancellationToken);

        return result.MatchDeleted(this);
    }
}

