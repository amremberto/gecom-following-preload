using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document type-related operations.
/// </summary>
internal sealed class DocumentTypeService : IDocumentTypeService
{
    private readonly IHttpClientService _httpClientService;
    private readonly PreloadApiSettings _apiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeService"/> class.
    /// </summary>
    /// <param name="httpClientService">The HTTP client service.</param>
    /// <param name="apiSettings">The API settings.</param>
    public DocumentTypeService(
        IHttpClientService httpClientService,
        IOptions<PreloadApiSettings> apiSettings)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        ArgumentNullException.ThrowIfNull(apiSettings);
        _apiSettings = apiSettings.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentTypeResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/DocumentTypes", UriKind.Relative);

        IEnumerable<DocumentTypeResponse>? response = await _httpClientService.GetAsync<IEnumerable<DocumentTypeResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }
}





