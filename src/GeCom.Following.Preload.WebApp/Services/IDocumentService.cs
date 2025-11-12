using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document-related operations.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Gets all documents from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    Task<IEnumerable<DocumentResponse>?> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range and optionally by provider CUIT from the API.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="providerCuit">Provider CUIT (optional). If not provided, returns documents from all providers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents matching the criteria.</returns>
    Task<IEnumerable<DocumentResponse>?> GetByDatesAndProviderAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        string? providerCuit = null,
        CancellationToken cancellationToken = default);
}

