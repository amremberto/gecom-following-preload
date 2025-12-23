using GeCom.Following.Preload.Domain.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for SapProviderSocietiy entities.
/// </summary>
public interface ISapProviderSocietiyRepository : IRepository<SapProviderSocietiy>
{
    /// <summary>
    /// Gets all society identifiers (Sociedadfi) associated with a provider account number.
    /// </summary>
    /// <param name="providerAccountNumber">The provider account number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of distinct society identifiers.</returns>
    Task<IReadOnlyList<string>> GetSocietyFiByProviderAccountNumberAsync(string providerAccountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all provider account numbers (Proveedor) associated with a society identifier (Sociedadfi).
    /// </summary>
    /// <param name="societyFi">The society identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of distinct provider account numbers.</returns>
    Task<IReadOnlyList<string>> GetProviderAccountNumbersBySocietyFiAsync(string societyFi, CancellationToken cancellationToken = default);
}
