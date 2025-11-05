namespace GeCom.Following.Preload.WebAPI.Configurations.Settings;

/// <summary>
/// Strongly-typed settings for IdentityServer options used by the API.
/// </summary>
public sealed class IdentityServerSettings
{
    public string Authority { get; set; } = string.Empty;

    public string MetadataAddress { get; set; } = string.Empty;

    public string ApiAudience { get; set; } = string.Empty;

    public string[] RequiredScopes { get; set; } = Array.Empty<string>();

    public bool RequireHttpsMetadata { get; set; } = true;

    public string? SwaggerClientId { get; set; }

    public bool SwaggerUsePkce { get; set; } = true;

    // Optional explicit OIDC settings for Swagger UI
    public string? OidcSwaggerUIClientId { get; set; }

    public string? OidcApiName { get; set; }
}


