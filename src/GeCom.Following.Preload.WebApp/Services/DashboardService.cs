using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for dashboard-related operations.
/// </summary>
internal sealed class DashboardService : IDashboardService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public DashboardService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<DashboardResponse?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Dashboard", UriKind.Relative);

        DashboardResponse? response = await _httpClientService.GetAsync<DashboardResponse>(
            requestUri,
            cancellationToken);

        return response;
    }
}

