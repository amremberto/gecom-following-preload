using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for DocumentType entities.
/// </summary>
public interface IDocumentTypeRepository : IRepository<DocumentType>
{
    /// <summary>
    /// Gets a document type by its code.
    /// </summary>
    /// <param name="code">Document type code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document type or null.</returns>
    Task<DocumentType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a document type by its TipoDocId.
    /// </summary>
    /// <param name="tipoDocId">Document type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document type or null.</returns>
    Task<DocumentType?> GetByTipoDocIdAsync(int tipoDocId, CancellationToken cancellationToken = default);
}
