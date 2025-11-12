namespace GeCom.Following.Preload.WebApp.Configurations.Settings;

/// <summary>
/// Strongly-typed settings for Preload WebAPI configuration used by the WebApp.
/// </summary>
public sealed class PreloadApiSettings
{
    /// <summary>
    /// Gets or sets the base URL of the WebAPI.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API version to use (e.g., "v1").
    /// </summary>
    public string Version { get; set; } = "v1";
}

