using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using GeCom.Following.Preload.WebAPI.Configurations.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApi.Configurations;

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

        // Register CORS settings
        services.Configure<CorsSettings>(configuration.GetSection("Cors"));

        // Register Swagger settings
        services.Configure<ApiSwaggerSettings>(configuration.GetSection("Swagger"));

        // Register Authentication settings
        services.Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));

        // Register IdentityServer settings
        services.Configure<IdentityServerSettings>(configuration.GetSection("IdentityServer"));

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

        // Register CORS settings validator
        services.AddSingleton<IValidateOptions<CorsSettings>>(_ =>
            new CorsSettingsValidator(environment));

        // Register Swagger settings validator
        services.AddSingleton<IValidateOptions<ApiSwaggerSettings>>(_ =>
            new SwaggerSettingsValidator());

        // Register logger settings validator (placeholder)
        services.AddSingleton<IValidateOptions<object>>(_ =>
            new LoggerSettingsValidator(environment));

        // Register database settings validator (placeholder)
        services.AddSingleton<IValidateOptions<object>>(_ =>
            new DatabaseSettingsValidator(environment));

        // Register Authentication settings validator
        services.AddSingleton<IValidateOptions<AuthenticationSettings>>(_ =>
            new AuthenticationSettingsValidator());

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
            // Validate CORS settings
            IOptions<CorsSettings>? corsOptions = app.Services.GetRequiredService<IOptions<CorsSettings>>();
            IValidateOptions<CorsSettings>? corsValidator = app.Services.GetRequiredService<IValidateOptions<CorsSettings>>();
            ValidateOptionsResult? corsResult = corsValidator.Validate(null, corsOptions.Value);

            if (corsResult.Failed)
            {
                throw new InvalidOperationException($"CORS configuration validation failed: {string.Join(", ", corsResult.Failures)}");
            }

            // Validate Swagger settings
            IOptions<ApiSwaggerSettings> apiSwaggerOptions = app.Services.GetRequiredService<IOptions<ApiSwaggerSettings>>();
            IValidateOptions<ApiSwaggerSettings> apiSwaggerValidator = app.Services.GetRequiredService<IValidateOptions<ApiSwaggerSettings>>();
            ValidateOptionsResult swaggerResult = apiSwaggerValidator.Validate(null, apiSwaggerOptions.Value);
            if (swaggerResult.Failed)
            {
                throw new InvalidOperationException($"Swagger configuration validation failed: {string.Join(", ", swaggerResult.Failures)}");
            }

            // Validate Authentication settings
            IOptions<AuthenticationSettings> authOptions = app.Services.GetRequiredService<IOptions<AuthenticationSettings>>();
            IValidateOptions<AuthenticationSettings> authValidator = app.Services.GetRequiredService<IValidateOptions<AuthenticationSettings>>();
            ValidateOptionsResult authResult = authValidator.Validate(null, authOptions.Value);
            if (authResult.Failed)
            {
                throw new InvalidOperationException($"Authentication configuration validation failed: {string.Join(", ", authResult.Failures)}");
            }

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
