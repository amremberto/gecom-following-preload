using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for provider-related operations.
/// </summary>
internal sealed class ProviderService : IProviderService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public ProviderService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProviderResponse>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Providers", UriKind.Relative);

        IEnumerable<ProviderResponse>? response = await _httpClientService.GetAsync<IEnumerable<ProviderResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<ProviderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Providers/id/{id}", UriKind.Relative);

        ProviderResponse? response = await _httpClientService.GetAsync<ProviderResponse>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<ProviderResponse?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Providers/cuit/{Uri.EscapeDataString(cuit)}", UriKind.Relative);

        ProviderResponse? response = await _httpClientService.GetAsync<ProviderResponse>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProviderResponse>?> SearchAsync(string searchText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        string apiVersion = _apiSettings.Version;
        string encodedSearchText = Uri.EscapeDataString(searchText);
        Uri requestUri = new($"/api/{apiVersion}/Providers/search?searchText={encodedSearchText}", UriKind.Relative);

        IEnumerable<ProviderResponse>? response = await _httpClientService.GetAsync<IEnumerable<ProviderResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }
}

