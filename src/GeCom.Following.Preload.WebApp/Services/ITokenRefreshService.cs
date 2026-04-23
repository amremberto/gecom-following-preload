namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service that ensures a valid access token is available for API calls.
/// </summary>
public interface ITokenRefreshService
{
    /// <summary>
    /// Ensures a valid access token exists in the current authentication session.
    /// </summary>
    /// <param name="forceRefresh">True to force refresh even if token is not near expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result containing token and session status.</returns>
    Task<TokenRefreshResult> EnsureValidAccessTokenAsync(
        bool forceRefresh,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result for token refresh operations.
/// </summary>
public sealed record TokenRefreshResult(
    bool IsSuccess,
    string? AccessToken,
    bool RequiresSignOut,
    string? Error);
