using System.Net;
using System.Net.Http.Headers;
using GeCom.Following.Preload.WebApp.Services;

namespace GeCom.Following.Preload.WebApp.Services.Handlers;

/// <summary>
/// Delegating handler that automatically adds the JWT access token to all HTTP requests.
/// </summary>
internal sealed class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly ILogger<AuthenticationDelegatingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationDelegatingHandler"/> class.
    /// </summary>
    public AuthenticationDelegatingHandler(
        ITokenRefreshService tokenRefreshService,
        ILogger<AuthenticationDelegatingHandler> logger)
    {
        _tokenRefreshService = tokenRefreshService ?? throw new ArgumentNullException(nameof(tokenRefreshService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage? retryRequest = await CloneRequestAsync(request, cancellationToken);
        TokenRefreshResult tokenResult = await _tokenRefreshService.EnsureValidAccessTokenAsync(forceRefresh: false, cancellationToken);
        if (!string.IsNullOrWhiteSpace(tokenResult.AccessToken) && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || retryRequest is null)
        {
            return response;
        }

        response.Dispose();
        _logger.LogWarning("Received 401 from API. Attempting a forced token refresh and one retry.");

        TokenRefreshResult forcedRefreshResult = await _tokenRefreshService.EnsureValidAccessTokenAsync(forceRefresh: true, cancellationToken);
        if (!forcedRefreshResult.IsSuccess || string.IsNullOrWhiteSpace(forcedRefreshResult.AccessToken))
        {
            _logger.LogWarning("Forced token refresh failed. Returning 401 to trigger re-authentication flow.");
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = request
            };
        }

        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", forcedRefreshResult.AccessToken);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private static async Task<HttpRequestMessage?> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (KeyValuePair<string, object?> option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is null)
        {
            return clone;
        }

        byte[] contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentClone = new ByteArrayContent(contentBytes);

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
        {
            contentClone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        clone.Content = contentClone;
        return clone;
    }
}

