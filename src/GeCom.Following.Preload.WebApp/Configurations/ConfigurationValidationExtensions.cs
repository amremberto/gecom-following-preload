using GeCom.Following.Preload.WebApp.Configurations.Settings;
using GeCom.Following.Preload.WebApp.Configurations.Validators;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Configurations;

/// <summary>
/// Extension methods for registering configuration validators.
/// </summary>
public static class ConfigurationValidationExtensions
{
    /// <summary>
    /// Registers all configuration settings with IOptions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurationSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register IdentityServer settings
        services.Configure<IdentityServerSettings>(configuration.GetSection("IdentityServer"));

        // Register Api settings
        services.Configure<PreloadApiSettings>(configuration.GetSection("PreloadApi"));

        return services;
    }

    /// <summary>
    /// Adds configuration validators for all settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The host environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurationValidators(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environment);

        // Register IdentityServer settings validator
        services.AddSingleton<IValidateOptions<IdentityServerSettings>>(_ =>
            new IdentityServerSettingsValidator());

        return services;
    }

    /// <summary>
    /// Validates all configuration options at startup.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ValidateConfiguration(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        try
        {
            // Validate IdentityServer settings
            IOptions<IdentityServerSettings> idsrvOptions = app.Services.GetRequiredService<IOptions<IdentityServerSettings>>();
            IValidateOptions<IdentityServerSettings> idsrvValidator = app.Services.GetRequiredService<IValidateOptions<IdentityServerSettings>>();
            ValidateOptionsResult idsrvResult = idsrvValidator.Validate(null, idsrvOptions.Value);
            if (idsrvResult.Failed)
            {
                throw new InvalidOperationException($"IdentityServer configuration validation failed: {string.Join(", ", idsrvResult.Failures)}");
            }

            return app;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Configuration validation failed: {ex.Message}", ex);
        }
    }
}

