using System.Net;
using Microsoft.AspNetCore.Components.Forms;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for making HTTP requests to the WebAPI with automatic JWT authentication.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Sends a GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response, or null if not found.</returns>
    Task<TResponse?> GetAsync<TResponse>(Uri requestUri, CancellationToken cancellationToken = default)
        where TResponse : class;

    /// <summary>
    /// Sends a POST request with a body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// Sends a PUT request with a body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse?> PutAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the request was successful, false otherwise.</returns>
    Task<bool> DeleteAsync(Uri requestUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request with a file (multipart/form-data) and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="fileParameterName">The name of the file parameter (default: "file").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse?> PostFileAsync<TResponse>(
        Uri requestUri,
        IBrowserFile file,
        string fileParameterName = "file",
        CancellationToken cancellationToken = default)
        where TResponse : class;

    /// <summary>
    /// Downloads a file as a byte array.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]?> DownloadFileAsync(Uri requestUri, CancellationToken cancellationToken = default);
}

