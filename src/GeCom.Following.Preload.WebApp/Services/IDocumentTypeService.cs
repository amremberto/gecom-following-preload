using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document type-related operations.
/// </summary>
public interface IDocumentTypeService
{
    /// <summary>
    /// Gets all document types from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of document types.</returns>
    Task<IEnumerable<DocumentTypeResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default);
}






