using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for SAP purchase order-related operations.
/// </summary>
internal sealed class SapPurchaseOrderService : ISapPurchaseOrderService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SapPurchaseOrderService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public SapPurchaseOrderService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SapPurchaseOrderResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/SapPurchaseOrders", UriKind.Relative);

        IEnumerable<SapPurchaseOrderResponse>? response = await _httpClientService.GetAsync<IEnumerable<SapPurchaseOrderResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }
}
