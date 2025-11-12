using GeCom.Following.Preload.Domain.Preloads.Providers;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Provider entities.
/// </summary>
public interface IProviderRepository : IRepository<Provider>
{
    /// <summary>
    /// Gets a provider by its CUIT.
    /// </summary>
    /// <param name="cuit">Provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider or null.</returns>
    Task<Provider?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for providers by a search text.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of providers that match the search text.</returns>
    Task<IEnumerable<Provider>> SearchAsync(string searchText, int maxResults = 20, CancellationToken cancellationToken = default);
}

