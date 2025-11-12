using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace GeCom.Following.Preload.WebApp.Services.Handlers;

/// <summary>
/// Delegating handler that automatically adds the JWT access token to all HTTP requests.
/// </summary>
internal sealed class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationDelegatingHandler"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            // Get the access token from the authentication cookie
            string? accessToken = await httpContext.GetTokenAsync(
                OpenIdConnectDefaults.AuthenticationScheme,
                "access_token");

            // Add the authorization header if not already present
            if (!string.IsNullOrWhiteSpace(accessToken) && request.Headers.Authorization is null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

