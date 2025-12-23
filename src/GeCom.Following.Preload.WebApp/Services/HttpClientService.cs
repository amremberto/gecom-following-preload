using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;

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
        using HttpResponseMessage response = await _httpClient.DeleteAsync(requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("The request was unauthorized. Please sign in again.");
        }

        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc />
    public async Task<TResponse?> PostFileAsync<TResponse>(
        Uri requestUri,
        IBrowserFile file,
        string fileParameterName = "file",
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileParameterName);

        using MultipartFormDataContent content = new();

        // Read file content into a stream
        await using Stream fileStream = file.OpenReadStream(maxAllowedSize: 6 * 1024 * 1024, cancellationToken); // 6 MB max
        using StreamContent fileContent = new(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, fileParameterName, file.Name);

        HttpResponseMessage response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResponse?> PutFileAsync<TResponse>(
        Uri requestUri,
        IBrowserFile file,
        string fileParameterName = "file",
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileParameterName);

        using MultipartFormDataContent content = new();

        // Read file content into a stream
        await using Stream fileStream = file.OpenReadStream(maxAllowedSize: 6 * 1024 * 1024, cancellationToken); // 6 MB max
        using StreamContent fileContent = new(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, fileParameterName, file.Name);

        HttpResponseMessage response = await _httpClient.PutAsync(requestUri, content, cancellationToken);

        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]?> DownloadFileAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                throw new UnauthorizedAccessException("The request was unauthorized. Please sign in again.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response.Dispose();
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
                response.Dispose();
                throw new ApiRequestException(response.StatusCode, errorMessage);
            }

            byte[] fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            response.Dispose();

            return fileContent;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
        {
            throw new ApiRequestException(
                HttpStatusCode.RequestTimeout,
                "La descarga del archivo tardó demasiado tiempo. Por favor, intente nuevamente.");
        }
        catch (OperationCanceledException)
        {
            throw new ApiRequestException(
                HttpStatusCode.RequestTimeout,
                "La descarga del archivo fue cancelada debido a un timeout.");
        }
    }

    /// <summary>
    /// Handles the HTTP response and deserializes it if successful.
    /// </summary>
    private static async Task<TResponse?> HandleResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        try
        {
            // Handle special cases before ensuring success status
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("The request was unauthorized. Please sign in again.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // Handle BadRequest and other client errors by extracting ProblemDetails message
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await ExtractErrorMessageAsync(response, cancellationToken);
                throw new ApiRequestException(response.StatusCode, errorMessage);
            }

            // Deserialize directly from the response content
            TResponse? result = await response.Content.ReadFromJsonAsync<TResponse>(
                JsonSerializerOptions,
                cancellationToken);

            return result;
        }
        finally
        {
            // Ensure response is disposed even if an exception occurs
            response.Dispose();
        }
    }

    /// <summary>
    /// Extracts the error message from the response body, attempting to deserialize ProblemDetails.
    /// </summary>
    private static async Task<string> ExtractErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
            {
                return $"The request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
            }

            // Try to parse as JSON
            try
            {
                using var doc = JsonDocument.Parse(content);
                JsonElement root = doc.RootElement;

                // Check if it's a ProblemDetails response
                if (root.TryGetProperty("detail", out JsonElement detailElement))
                {
                    string? detail = detailElement.GetString();
                    if (!string.IsNullOrWhiteSpace(detail))
                    {
                        return detail;
                    }
                }

                // Fallback to title if detail is not available
                if (root.TryGetProperty("title", out JsonElement titleElement))
                {
                    string? title = titleElement.GetString();
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        return title;
                    }
                }

                // If it's a simple string response (like our BadRequest messages)
                if (root.ValueKind == JsonValueKind.String)
                {
                    string? stringValue = root.GetString();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        return stringValue;
                    }
                }
            }
            catch (JsonException)
            {
                // If it's not valid JSON, it might be a plain string
                // Remove surrounding quotes if present
                content = content.Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return content;
                }
            }

            // Last resort: return generic message
            return $"The request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
        }
        catch
        {
            // If we can't parse the error, return a generic message
            return $"The request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";
        }
    }
}

