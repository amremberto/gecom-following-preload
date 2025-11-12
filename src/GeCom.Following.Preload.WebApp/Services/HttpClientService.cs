using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for making HTTP requests to the WebAPI with automatic JWT authentication.
/// </summary>
internal sealed class HttpClientService : IHttpClientService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public HttpClientService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<TResponse?> GetAsync<TResponse>(
        Uri requestUri,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        string jsonContent = JsonSerializer.Serialize(request, JsonSerializerOptions);
        using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        string jsonContent = JsonSerializer.Serialize(request, JsonSerializerOptions);
        using StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(requestUri, content, cancellationToken);

        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(requestUri, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("The request was unauthorized. Please sign in again.");
        }

        response.EnsureSuccessStatusCode();
        return false;
    }

    /// <summary>
    /// Handles the HTTP response and deserializes it if successful.
    /// </summary>
    private static async Task<TResponse?> HandleResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        if (response.IsSuccessStatusCode)
        {
            string jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return null;
            }

            TResponse? result = JsonSerializer.Deserialize<TResponse>(jsonContent, JsonSerializerOptions);
            return result;
        }

        // Handle error responses
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("The request was unauthorized. Please sign in again.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return null;
    }
}

