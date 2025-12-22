using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for SAP provider society-related operations.
/// </summary>
internal sealed class SapProviderSocietyService : ISapProviderSocietyService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SapProviderSocietyService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public SapProviderSocietyService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProviderSocietyResponse>?> GetSocietiesByProviderCuitAsync(
        string providerCuit,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerCuit);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/SapProviderSocieties/provider/{Uri.EscapeDataString(providerCuit)}/societies", UriKind.Relative);

        IEnumerable<ProviderSocietyResponse>? response = await _httpClientService.GetAsync<IEnumerable<ProviderSocietyResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProviderSocietyResponse>?> GetSocietiesByUserEmailAsync(
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/SapProviderSocieties/user/{Uri.EscapeDataString(userEmail)}/societies", UriKind.Relative);

        IEnumerable<ProviderSocietyResponse>? response = await _httpClientService.GetAsync<IEnumerable<ProviderSocietyResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProviderSocietyResponse>?> GetProvidersBySocietyCuitAsync(
        string societyCuit,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(societyCuit);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/SapProviderSocieties/society/{Uri.EscapeDataString(societyCuit)}/providers", UriKind.Relative);

        IEnumerable<ProviderSocietyResponse>? response = await _httpClientService.GetAsync<IEnumerable<ProviderSocietyResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }
}
