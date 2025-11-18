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
    /// Gets documents by emission date range and provider CUIT from the API.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="providerCuit">Provider CUIT (required). Must match the CUIT in the user's claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents matching the criteria.</returns>
    Task<IEnumerable<DocumentResponse>?> GetByDatesAndProviderAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        string providerCuit,
        CancellationToken cancellationToken = default);
}

