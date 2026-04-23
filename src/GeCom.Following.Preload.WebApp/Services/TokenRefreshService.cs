using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Services;

internal sealed class TokenRefreshService : ITokenRefreshService
{
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromSeconds(60);
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<IdentityServerSettings> _identityServerOptions;
    private readonly ILogger<TokenRefreshService> _logger;

    public TokenRefreshService(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerSettings> identityServerOptions,
        ILogger<TokenRefreshService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _identityServerOptions = identityServerOptions ?? throw new ArgumentNullException(nameof(identityServerOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TokenRefreshResult> EnsureValidAccessTokenAsync(
        bool forceRefresh,
        CancellationToken cancellationToken = default)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return new TokenRefreshResult(false, null, false, "HttpContext is not available.");
        }

        AuthenticateResult authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal is null || authResult.Properties is null)
        {
            return new TokenRefreshResult(false, null, true, "User session is not authenticated.");
        }

        string? accessToken = authResult.Properties.GetTokenValue("access_token");
        string? refreshToken = authResult.Properties.GetTokenValue("refresh_token");
        string? expiresAt = authResult.Properties.GetTokenValue("expires_at");

        bool mustRefresh = forceRefresh || IsAccessTokenNearExpiry(expiresAt);
        if (!mustRefresh && !string.IsNullOrWhiteSpace(accessToken))
        {
            return new TokenRefreshResult(true, accessToken, false, null);
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("No refresh_token available in the current session.");
            await SignOutLocalSessionAsync(httpContext);
            return new TokenRefreshResult(false, null, true, "Refresh token is missing.");
        }

        TokenEndpointResult tokenResult = await RefreshAccessTokenAsync(refreshToken, cancellationToken);
        if (!tokenResult.IsSuccess || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
        {
            _logger.LogWarning("Token refresh failed. Error: {Error}", tokenResult.Error);
            await SignOutLocalSessionAsync(httpContext);
            return new TokenRefreshResult(false, null, true, tokenResult.Error ?? "Token refresh failed.");
        }

        string refreshedRefreshToken = string.IsNullOrWhiteSpace(tokenResult.RefreshToken)
            ? refreshToken
            : tokenResult.RefreshToken;

        DateTimeOffset newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpiresIn);
        IEnumerable<AuthenticationToken> newTokens =
        [
            new AuthenticationToken { Name = "access_token", Value = tokenResult.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = refreshedRefreshToken },
            new AuthenticationToken { Name = "expires_at", Value = newExpiresAt.ToString("o", CultureInfo.InvariantCulture) }
        ];

        string? idToken = authResult.Properties.GetTokenValue("id_token");
        if (!string.IsNullOrWhiteSpace(idToken))
        {
            newTokens = newTokens.Append(new AuthenticationToken { Name = "id_token", Value = idToken });
        }

        authResult.Properties.StoreTokens(newTokens);

        // In Blazor Server/component pipelines, the response can already be started.
        // In that case headers are read-only and cookie re-issue would throw.
        if (!httpContext.Response.HasStarted)
        {
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authResult.Principal,
                authResult.Properties);
        }
        else
        {
            _logger.LogWarning(
                "Skipping cookie re-issue after token refresh because response has already started for user {User}.",
                authResult.Principal.Identity?.Name);
        }

        _logger.LogInformation("Access token refreshed successfully for user {User}.", authResult.Principal.Identity?.Name);
        return new TokenRefreshResult(true, tokenResult.AccessToken, false, null);
    }

    private async Task<TokenEndpointResult> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        IdentityServerSettings settings = _identityServerOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.Authority) || string.IsNullOrWhiteSpace(settings.ClientId))
        {
            return new TokenEndpointResult(false, null, null, 0, "IdentityServer settings are incomplete.");
        }

        using HttpClient tokenClient = _httpClientFactory.CreateClient("OidcTokenClient");
        string tokenEndpoint = $"{settings.Authority.TrimEnd('/')}/connect/token";

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(BuildRefreshRequest(settings, refreshToken))
        };

        using HttpResponseMessage response = await tokenClient.SendAsync(request, cancellationToken);
        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new TokenEndpointResult(false, null, null, 0, responseContent);
        }

        using var document = JsonDocument.Parse(responseContent);
        JsonElement root = document.RootElement;

        string? accessToken = root.TryGetProperty("access_token", out JsonElement accessElement)
            ? accessElement.GetString()
            : null;
        string? refreshedRefreshToken = root.TryGetProperty("refresh_token", out JsonElement refreshElement)
            ? refreshElement.GetString()
            : null;
        int expiresIn = root.TryGetProperty("expires_in", out JsonElement expiresElement) && expiresElement.TryGetInt32(out int value)
            ? value
            : 3600;

        return new TokenEndpointResult(true, accessToken, refreshedRefreshToken, expiresIn, null);
    }

    private static Dictionary<string, string> BuildRefreshRequest(IdentityServerSettings settings, string refreshToken)
    {
        Dictionary<string, string> data = new(StringComparer.Ordinal)
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = settings.ClientId,
            ["refresh_token"] = refreshToken
        };

        if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            data["client_secret"] = settings.ClientSecret;
        }

        return data;
    }

    private static bool IsAccessTokenNearExpiry(string? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(expiresAt))
        {
            return true;
        }

        bool parsed = DateTimeOffset.TryParse(
            expiresAt,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTimeOffset expiry);

        if (!parsed)
        {
            return true;
        }

        return expiry <= DateTimeOffset.UtcNow.Add(RefreshThreshold);
    }

    private static async Task SignOutLocalSessionAsync(HttpContext httpContext)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private sealed record TokenEndpointResult(
        bool IsSuccess,
        string? AccessToken,
        string? RefreshToken,
        int ExpiresIn,
        string? Error);
}
