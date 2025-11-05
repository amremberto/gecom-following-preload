using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Note entities.
/// </summary>
public interface INoteRepository : IRepository<Note>
{
    /// <summary>
    /// Gets a note by its NotaId.
    /// </summary>
    /// <param name="notaId">Note ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The note or null.</returns>
    Task<Note?> GetByNotaIdAsync(int notaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all notes for a specific document.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of notes for the document.</returns>
    Task<IEnumerable<Note>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notes by user who created them.
    /// </summary>
    /// <param name="usuarioCreacion">User who created the notes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of notes created by the user.</returns>
    Task<IEnumerable<Note>> GetByUsuarioCreacionAsync(string usuarioCreacion, CancellationToken cancellationToken = default);
}
