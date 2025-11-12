using GeCom.Following.Preload.Application;
using GeCom.Following.Preload.Infrastructure;
using GeCom.Following.Preload.Infrastructure.Logging.Serilog;
using GeCom.Following.Preload.WebApi.Configurations;
using GeCom.Following.Preload.WebApi.Extensions.Auth;
using GeCom.Following.Preload.WebApi.Extensions.OpenApi;
using GeCom.Following.Preload.WebApi.Extensions.Versioning;
using GeCom.Following.Preload.WebApi.Middlewares;
using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using GeCom.Security.Authorization.WebApi.Extensions.Cors;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using Serilog;

// Initialize the logger with a bootstrap logger, to log in console before the configuration is loaded.
SerilogConfigurator.InitializeBootstrapLogger();

try
{
    Log.Information("Application starting up...");

    WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

    #region Add services to the container.

    // Loads the configuration from the appsettings.json and configuration json files.
    ConfigurationsLoader.AddConfigurationJsonFiles(builder.Configuration, builder.Environment);
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(true);
    }

    // Register configuration settings with IOptions
    builder.Services.AddConfigurationSettings(builder.Configuration);

    // Add configuration validators (immediately after loading configurations)
    builder.Services.AddConfigurationValidators(builder.Environment);

    // Configures the logger with the provided configuration in the logger.json configuration file.
    SerilogConfigurator.ConfigureLogger(builder.Configuration);
    // Adds Serilog to the host builder.
    SerilogConfigurator.AddSerilogToHostBuilder(builder.Host);

    // Add memory cache for dashboard and other cached responses
    builder.Services.AddMemoryCache();

    // Platform services - add controller services
    builder.Services.AddControllers();

    // Add API versioning + versioned explorer
    builder.Services.AddApiVersioningWithExplorer();

    // Add OpenAPI / Swagger (NSwag) with settings
    builder.Services.AddOpenApiDocumentation(builder.Configuration);

    // Add CORS
    builder.Services.AddAdminApiCors(builder.Configuration, builder.Environment);

    // Add application services.
    builder.Services.AddPreloadApplication();

    // Add infrastructure services.
    builder.Services.AddPreloadInfrastructure(builder.Configuration);

    // Add JWT authentication services
    builder.Services.AddPreloadAuthentication(builder.Configuration);

    // Add authorization services with custom policies
    builder.Services.AddPreloadAuthorization();

    // Add health checks
    string? authorizationConnectionString = builder.Configuration.GetConnectionString("PreloadConnection");
    if (!string.IsNullOrWhiteSpace(authorizationConnectionString))
    {
        builder.Services
            .AddHealthChecks()
            .AddSqlServer(
                authorizationConnectionString,
                "sqlserver-authz",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "authz"]);
    }
    else
    {
        builder.Services.AddHealthChecks(); // sin checks si no hay connection string a�n
    }

    #endregion

    WebApplication? app = builder.Build();

    // Validate configuration at startup
    app.ValidateConfiguration();

    #region Configure the HTTP request pipeline.

    // Adds middleware for streamlined request logging. 
    app.UseSerilogRequestLogging();

    // Use the developer exception page in development environment
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        // Use custom ProblemDetails middleware for global exception handling
        app.UseProblemDetails();
    }

    // Enforce HTTPS redirection
    app.UseHttpsRedirection();

    // Enable middleware to serve generated OpenAPI as a JSON endpoint and the Swagger UI
    ApiSwaggerSettings? swaggerSettings = app.Services.GetRequiredService<IOptions<ApiSwaggerSettings>>().Value;
    if (swaggerSettings.Enabled)
    {
        app.UseOpenApi();
        app.UseSwaggerUi(settings =>
        {
            settings.DocumentTitle = swaggerSettings.Title + " - Swagger";
            settings.Path = swaggerSettings.Route;
            settings.DocumentPath = swaggerSettings.JsonRoute;

            // Configure OAuth2 client (Authorization Code + PKCE) for Swagger UI
            IdentityServerSettings? identityServerSettings = app.Services.GetRequiredService<IOptions<IdentityServerSettings>>().Value;
            settings.OAuth2Client = new OAuth2ClientSettings
            {
                ClientId = identityServerSettings.OidcSwaggerUIClientId ?? identityServerSettings.SwaggerClientId ?? string.Empty,
                AppName = swaggerSettings.Title,
                UsePkceWithAuthorizationCodeGrant = identityServerSettings.SwaggerUsePkce
            };

            foreach (string scope in identityServerSettings.RequiredScopes ?? [])
            {
                settings.OAuth2Client.Scopes.Add(scope);
            }

            // Removed audience parameter injection for Duende compatibility
        });
    }

    // Use CORS with configured policy name from IOptions
    CorsSettings? corsSettings = app.Services.GetRequiredService<IOptions<CorsSettings>>().Value;
    string policyName = string.IsNullOrWhiteSpace(corsSettings.PolicyName) ? "DefaultCorsPolicy" : corsSettings.PolicyName;
    app.UseCors(policyName);

    // Use authentication
    app.UseAuthentication();
    // Use authorization
    app.UseAuthorization();

    // Map health checks endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Map controllers
    app.MapControllers();

    Log.Information("Application configured successfully...");

    #endregion

    await app.RunAsync();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    SerilogConfigurator.InitializeBootstrapLogger();
    Log.Fatal(ex, "Unhandled exception - The application failed to start...");
}
finally
{
    Log.Information("Application Shutting down...");
    await Log.CloseAndFlushAsync();
}
