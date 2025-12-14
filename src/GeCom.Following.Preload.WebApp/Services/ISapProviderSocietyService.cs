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
}
