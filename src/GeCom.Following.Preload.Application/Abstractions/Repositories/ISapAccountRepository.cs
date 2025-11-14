using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for SapAccount entities.
/// </summary>
public interface ISapAccountRepository : IRepository<SapAccount>
{
    /// <summary>
    /// Gets a SAP account by its account number.
    /// </summary>
    /// <param name="accountNumber">Account number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SAP account or null.</returns>
    Task<SapAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a SAP account by its account number with tracking enabled (for updates).
    /// </summary>
    /// <param name="accountNumber">Account number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SAP account or null.</returns>
    Task<SapAccount?> GetByAccountNumberForUpdateAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a SAP account by its CUIT.
    /// </summary>
    /// <param name="cuit">Account CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SAP account or null.</returns>
    Task<SapAccount?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a SAP account by its account number.
    /// </summary>
    /// <param name="accountNumber">Account number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
}

