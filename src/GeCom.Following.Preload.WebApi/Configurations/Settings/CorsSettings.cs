namespace GeCom.Following.Preload.WebAPI.Configurations.Settings;

/// <summary>
/// Strongly-typed settings for CORS configuration bound from configuration files.
/// </summary>
public sealed class CorsSettings
{
    public string? PolicyName { get; set; }

    public string[] AllowedOrigins { get; set; } = [];

    public string[] AllowedMethods { get; set; } = [];

    public string[] AllowedHeaders { get; set; } = [];

    public bool AllowCredentials { get; set; }

    public string[] ExposedHeaders { get; set; } = [];
}


