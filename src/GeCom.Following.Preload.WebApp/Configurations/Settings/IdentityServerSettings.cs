namespace GeCom.Following.Preload.WebApp.Configurations.Settings;

/// <summary>
/// Strongly-typed settings for IdentityServer options used by the WebApp.
/// </summary>
public sealed class IdentityServerSettings
{
    /// <summary>
    /// Gets or sets the IdentityServer authority URL.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata address URL.
    /// </summary>
    public string MetadataAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client ID for the WebApp.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for the WebApp (optional, required if client is not public).
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the required scopes for authentication.
    /// </summary>
    public string[] RequiredScopes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether HTTPS metadata is required.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets the redirect URI after successful authentication.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the post-logout redirect URI.
    /// </summary>
    public string PostLogoutRedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response type (default: "code" for Authorization Code flow).
    /// </summary>
    public string ResponseType { get; set; } = "code";

    /// <summary>
    /// Gets or sets a value indicating whether PKCE (Proof Key for Code Exchange) should be used.
    /// </summary>
    public bool UsePkce { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether tokens should be saved in the authentication cookie.
    /// </summary>
    public bool SaveTokens { get; set; } = true;
}

