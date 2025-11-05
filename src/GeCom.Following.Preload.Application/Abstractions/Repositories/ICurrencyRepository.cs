using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Currency entities.
/// </summary>
public interface ICurrencyRepository : IRepository<Currency>
{
    /// <summary>
    /// Gets a currency by its code.
    /// </summary>
    /// <param name="code">Currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The currency or null.</returns>
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}


