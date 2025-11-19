using System.Globalization;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document-related operations.
/// </summary>
internal sealed class DocumentService : IDocumentService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public DocumentService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentResponse>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Documents", UriKind.Relative);

        IEnumerable<DocumentResponse>? response = await _httpClientService.GetAsync<IEnumerable<DocumentResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentResponse>?> GetByDatesAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;

        // Build query string
        string queryString = $"dateFrom={Uri.EscapeDataString(dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}&dateTo={Uri.EscapeDataString(dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}";

        Uri requestUri = new($"/api/{apiVersion}/Documents/by-dates?{queryString}", UriKind.Relative);

        IEnumerable<DocumentResponse>? response = await _httpClientService.GetAsync<IEnumerable<DocumentResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentResponse>?> GetByDatesAndProviderAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        string providerCuit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(providerCuit);

        string apiVersion = _apiSettings.Version;

        // Build query string
        string queryString = $"dateFrom={Uri.EscapeDataString(dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}&dateTo={Uri.EscapeDataString(dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}&providerCuit={Uri.EscapeDataString(providerCuit)}";

        Uri requestUri = new($"/api/{apiVersion}/Documents/by-dates-and-provider?{queryString}", UriKind.Relative);

        IEnumerable<DocumentResponse>? response = await _httpClientService.GetAsync<IEnumerable<DocumentResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }
}

