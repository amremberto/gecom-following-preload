namespace GeCom.Following.Preload.WebAPI.Configurations.Settings;

/// <summary>
/// Strongly-typed settings for Swagger/OpenAPI configuration.
/// </summary>
public sealed class ApiSwaggerSettings
{
    public bool Enabled { get; set; } = true;

    public string Title { get; set; } = "API";

    public string Version { get; set; } = "v1";

    public string DocumentName { get; set; } = "v1";

    public string Route { get; set; } = "/swagger";

    public string JsonRoute { get; set; } = "/swagger/v1/swagger.json";

    public bool EnableOAuth2Security { get; set; } = true;
}


