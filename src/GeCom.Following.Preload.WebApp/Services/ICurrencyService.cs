using GeCom.Following.Preload.Contracts.Preload.Currencies;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for currency-related operations.
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Gets all currencies from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of currencies.</returns>
    Task<IEnumerable<CurrencyResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default);
}
