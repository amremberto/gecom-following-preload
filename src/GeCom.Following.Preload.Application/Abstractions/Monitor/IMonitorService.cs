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
    Task<int> GetSapDocumentNumberAsync(GetSapDocumentNumberRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all document IDs from [Monitores].[dbo].[Documents] that match the given SapDocumentNumber.
    /// </summary>
    /// <param name="sapDocumentNumber">The SAP document number to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of document IDs; empty if no matches.</returns>
    Task<IReadOnlyList<int>> GetDocumentIdsBySapDocumentNumberAsync(int sapDocumentNumber, CancellationToken cancellationToken = default);
}
