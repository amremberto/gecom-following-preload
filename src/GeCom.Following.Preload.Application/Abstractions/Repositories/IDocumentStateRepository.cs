using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for DocumentState entities.
/// </summary>
public interface IDocumentStateRepository : IRepository<DocumentState>
{
    /// <summary>
    /// Gets document states by document ID.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of document states.</returns>
    Task<IEnumerable<DocumentState>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets document states by state ID.
    /// </summary>
    /// <param name="estadoId">State ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of document states.</returns>
    Task<IEnumerable<DocumentState>> GetByEstadoIdAsync(int estadoId, CancellationToken cancellationToken = default);
}
