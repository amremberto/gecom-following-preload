using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for SAP purchase order-related operations.
/// </summary>
public interface ISapPurchaseOrderService
{
    /// <summary>
    /// Gets all SAP purchase orders from the API based on user role.
    /// The filtering is automatically handled by the backend based on user role:
    /// - Providers: Filters by provider account number (obtained from CUIT)
    /// - Societies: Filters by Sociedadfi codes from user's assigned societies
    /// - Administrator/ReadOnly: Returns all purchase orders
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of SAP purchase orders based on user role.</returns>
    Task<IEnumerable<SapPurchaseOrderResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets SAP purchase orders by provider CUIT, society CUIT, and document ID.
    /// </summary>
    /// <param name="providerCuit">The provider CUIT to filter by.</param>
    /// <param name="societyCuit">The society CUIT to filter by.</param>
    /// <param name="docId">The document ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of SAP purchase orders matching the specified criteria.</returns>
    Task<IEnumerable<SapPurchaseOrderResponse>?> GetByProviderSocietyAndDocIdAsync(
        string providerCuit,
        string societyCuit,
        int docId,
        CancellationToken cancellationToken = default);
}
