using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for currency-related operations.
/// </summary>
internal sealed class CurrencyService : ICurrencyService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public CurrencyService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CurrencyResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Currencies", UriKind.Relative);

        IEnumerable<CurrencyResponse>? response = await _httpClientService.GetAsync<IEnumerable<CurrencyResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }
}
