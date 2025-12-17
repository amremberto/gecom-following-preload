using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for SapPurchaseOrder entities.
/// </summary>
public interface ISapPurchaseOrderRepository : IRepository<SapPurchaseOrder>
{
    /// <summary>
    /// Gets all SAP purchase orders filtered by provider account number.
    /// </summary>
    /// <param name="providerAccountNumber">The provider account number to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of SAP purchase orders for the specified provider.</returns>
    Task<IEnumerable<SapPurchaseOrder>> GetByProviderAccountNumberAsync(string providerAccountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all SAP purchase orders filtered by society financial codes.
    /// </summary>
    /// <param name="sociedadFiCodes">The list of sociedad financial codes to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of SAP purchase orders for the specified societies.</returns>
    Task<IEnumerable<SapPurchaseOrder>> GetBySociedadFiCodesAsync(IReadOnlyList<string> sociedadFiCodes, CancellationToken cancellationToken = default);
}
