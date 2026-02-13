namespace GeCom.Following.Preload.Application.Abstractions.Monitor;

/// <summary>
/// Service for querying data in the Monitores database (e.g. SapDocumentNumber from [Monitores].[dbo].[Documents]).
/// </summary>
public interface IMonitorService
{
    /// <summary>
    /// Gets the SapDocumentNumber of a document in [Monitores].[dbo].[Documents] matching the given criteria.
    /// </summary>
    /// <param name="request">Filter by DocumentNumber, ProviderNumber, ClientNumber, SalePoint, Letter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SapDocumentNumber if a row exists; otherwise, null.</returns>
    Task<int?> GetSapDocumentNumberAsync(GetSapDocumentNumberRequest request, CancellationToken cancellationToken = default);
}
