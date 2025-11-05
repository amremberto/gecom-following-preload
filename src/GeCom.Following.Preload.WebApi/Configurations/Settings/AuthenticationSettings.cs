namespace GeCom.Following.Preload.WebAPI.Configurations.Settings;

/// <summary>
/// Strongly-typed settings for JWT authentication against Duende IdentityServer.
/// </summary>
public sealed class AuthenticationSettings
{
    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = true;
}


