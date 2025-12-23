using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for SAP provider society-related operations.
/// </summary>
public interface ISapProviderSocietyService
{
    /// <summary>
    /// Gets all societies that a provider can assign documents to.
    /// </summary>
    /// <param name="providerCuit">The provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of societies that the provider can assign documents to.</returns>
    Task<IEnumerable<ProviderSocietyResponse>?> GetSocietiesByProviderCuitAsync(
        string providerCuit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all societies that a user (by email) has access to.
    /// </summary>
    /// <param name="userEmail">The user email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of societies that the user has access to.</returns>
    Task<IEnumerable<ProviderSocietyResponse>?> GetSocietiesByUserEmailAsync(
        string userEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all providers that can assign documents to a specific society.
    /// </summary>
    /// <param name="societyCuit">The society CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of providers that can assign documents to the society.</returns>
    Task<IEnumerable<ProviderSocietyResponse>?> GetProvidersBySocietyCuitAsync(
        string societyCuit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets societies for the current user based on their role.
    /// The API automatically determines which societies to return based on the authenticated user's role.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of societies available for the current user based on their role.</returns>
    Task<IEnumerable<SocietySelectItemResponse>?> GetSocietiesForCurrentUserAsync(
        CancellationToken cancellationToken = default);
}
