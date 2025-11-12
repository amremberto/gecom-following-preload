using GeCom.Following.Preload.Contracts.Preload.Providers;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for provider-related operations.
/// </summary>
public interface IProviderService
{
    /// <summary>
    /// Gets all providers from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of providers.</returns>
    Task<IEnumerable<ProviderResponse>?> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a provider by its ID from the API.
    /// </summary>
    /// <param name="id">Provider ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider if found, null otherwise.</returns>
    Task<ProviderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a provider by its CUIT from the API.
    /// </summary>
    /// <param name="cuit">Provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider if found, null otherwise.</returns>
    Task<ProviderResponse?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for providers by search text from the API.
    /// </summary>
    /// <param name="searchText">Search text to match against provider CUIT, business name, or email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of providers matching the search criteria.</returns>
    Task<IEnumerable<ProviderResponse>?> SearchAsync(string searchText, CancellationToken cancellationToken = default);
}

