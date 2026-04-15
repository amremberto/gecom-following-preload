using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Contracts.Preload.Notes.Create;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for note-related operations.
/// </summary>
public interface INoteService
{
    /// <summary>
    /// Creates a note for a document.
    /// </summary>
    /// <param name="request">The note payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created note if successful.</returns>
    Task<NoteResponse?> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken = default);
}
