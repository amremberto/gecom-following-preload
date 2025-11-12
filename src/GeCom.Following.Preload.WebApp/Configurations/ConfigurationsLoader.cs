namespace GeCom.Following.Preload.WebApp.Configurations;

/// <summary>
/// This class is responsible for loading the configuration files.
/// </summary>
internal static class ConfigurationsLoader
{
    /// <summary>
    /// Load the configuration files to the configuration builder.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    /// <param name="env">The host environment.</param>
    internal static void AddConfigurationJsonFiles(IConfigurationBuilder configurationBuilder, IHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        ArgumentNullException.ThrowIfNull(env);

        const string configurationsDirectory = "Configurations/jsons";
        string environmentName = env.EnvironmentName;

        // appsettings raíz
        configurationBuilder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

        // lista de archivos de configuración por dominio
        string[] configNames =
        [
            "identityServer",
            "web-api"
        ];

        foreach (string name in configNames)
        {
            string basePath = $"{configurationsDirectory}/{name}.json";
            string envPath = $"{configurationsDirectory}/{name}.{environmentName}.json";

            configurationBuilder
                .AddJsonFile(basePath, optional: false, reloadOnChange: true)
                .AddJsonFile(envPath, optional: true, reloadOnChange: true);
        }

        configurationBuilder.AddEnvironmentVariables();
    }
}

