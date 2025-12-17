using System.Globalization;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Documents.Update;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.AspNetCore.Components.Forms;
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
    public async Task<IEnumerable<DocumentResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default)
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
    public async Task<IEnumerable<DocumentResponse>?> GetPaidDocumentsByDatesAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;

        // Build query string
        string queryString = $"dateFrom={Uri.EscapeDataString(dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}&dateTo={Uri.EscapeDataString(dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}";

        Uri requestUri = new($"/api/{apiVersion}/Documents/paid-by-dates?{queryString}", UriKind.Relative);

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

    /// <inheritdoc />
    public async Task<DocumentResponse?> GetByIdAsync(
        int docId,
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Documents/{docId}", UriKind.Relative);

        DocumentResponse? response = await _httpClientService.GetAsync<DocumentResponse>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<DocumentResponse?> PreloadDocumentAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Documents/preload", UriKind.Relative);

        DocumentResponse? response = await _httpClientService.PostFileAsync<DocumentResponse>(
            requestUri,
            file,
            "file",
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<byte[]?> DownloadAttachmentAsync(
        int adjuntoId,
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Attachments/{adjuntoId}/download", UriKind.Relative);

        byte[]? fileContent = await _httpClientService.DownloadFileAsync(requestUri, cancellationToken);

        return fileContent;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentResponse>?> GetPendingDocumentsByProviderAsync(
        string providerCuit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(providerCuit);

        string apiVersion = _apiSettings.Version;

        // Build query string
        string queryString = $"providerCuit={Uri.EscapeDataString(providerCuit)}";

        Uri requestUri = new($"/api/{apiVersion}/Documents/pending-by-provider?{queryString}", UriKind.Relative);

        IEnumerable<DocumentResponse>? response = await _httpClientService.GetAsync<IEnumerable<DocumentResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentResponse>?> GetPendingDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Documents/pending", UriKind.Relative);

        IEnumerable<DocumentResponse>? response = await _httpClientService.GetAsync<IEnumerable<DocumentResponse>>(
            requestUri,
            cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task<DocumentResponse?> UpdateAsync(
        int docId,
        UpdateDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string apiVersion = _apiSettings.Version;
        Uri requestUri = new($"/api/{apiVersion}/Documents/{docId}", UriKind.Relative);

        DocumentResponse? response = await _httpClientService.PutAsync<UpdateDocumentRequest, DocumentResponse>(
            requestUri,
            request,
            cancellationToken);

        return response;
    }
}

