namespace GeCom.Following.Preload.WebApi.Configurations;

/// <summary>
/// This class is responsible for loading the configuration files.
/// </summary>
internal static class ConfigurationsLoader
{
    /// <summary>
    /// Load the configuration files to the configuration builder.
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <param name="env"></param>
    internal static void AddConfigurationJsonFiles(IConfigurationBuilder configurationBuilder, IHostEnvironment env)
    {
        const string configurationsDirectory = "Configurations/jsons";
        string environmentName = env.EnvironmentName;

        // appsettings raíz
        configurationBuilder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

        // lista de archivos de configuración por dominio
        string[] configNames =
        [
            "application",
            "authentication",
            "cors",
            "database",
            "identityServer",
            "logger",
            "nswag",
            "storage"
        ];

        foreach (string name in configNames)
        {
            string basePath = $"{configurationsDirectory}/{name}.json";
            string envPath = $"{configurationsDirectory}/{name}.{environmentName}.json";

            // Marca algunos como obligatorios si lo deseas
            //bool isRequired = name is "database" or "logger"

            configurationBuilder
                .AddJsonFile(basePath, optional: false, reloadOnChange: true)
                .AddJsonFile(envPath, optional: true, reloadOnChange: true);
        }

        configurationBuilder.AddEnvironmentVariables();
    }
}
