using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.WebApp.Services.Societies;

/// <summary>
/// Service for society-related operations.
/// </summary>
public interface ISocietyService
{
    /// <summary>
    /// Gets all societies from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of societies.</returns>
    Task<IEnumerable<SocietyResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default);
}
