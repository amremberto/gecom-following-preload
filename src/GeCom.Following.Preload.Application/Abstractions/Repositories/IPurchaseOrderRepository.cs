using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for PurchaseOrder entities with specific business operations.
/// </summary>
public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    /// <summary>
    /// Gets purchase orders by document ID asynchronously.
    /// </summary>
    /// <param name="documentId">The document ID to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of purchase orders for the specified document.</returns>
    Task<IEnumerable<PurchaseOrder>> GetByDocumentIdAsync(int documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets purchase orders by provider SAP code asynchronously.
    /// </summary>
    /// <param name="providerSapCode">The provider SAP code to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of purchase orders for the specified provider.</returns>
    Task<IEnumerable<PurchaseOrder>> GetByProviderSapCodeAsync(string providerSapCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets purchase orders by society code asynchronously.
    /// </summary>
    /// <param name="societyCode">The society code to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of purchase orders for the specified society.</returns>
    Task<IEnumerable<PurchaseOrder>> GetBySocietyCodeAsync(string societyCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets purchase orders created within a date range asynchronously.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of purchase orders created within the specified date range.</returns>
    Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets purchase orders with their related documents asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of purchase orders with their related documents.</returns>
    Task<IEnumerable<PurchaseOrder>> GetWithDocumentsAsync(CancellationToken cancellationToken = default);
}
