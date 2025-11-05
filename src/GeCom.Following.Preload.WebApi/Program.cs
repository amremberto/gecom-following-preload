using GeCom.Following.Preload.Application;
using GeCom.Following.Preload.Infrastructure;
using GeCom.Following.Preload.Infrastructure.Logging.Serilog;
using GeCom.Following.Preload.WebApi.Configurations;
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

    // Add application services.
    builder.Services.AddPreloadApplication();

    // Add infrastructure services.
    builder.Services.AddPreloadInfrastructure(builder.Configuration);

    builder.Services.AddControllers();

    #endregion

    WebApplication? app = builder.Build();

    // Validate configuration at startup
    app.ValidateConfiguration();

    #region Configure the HTTP request pipeline.

    // Adds middleware for streamlined request logging. 
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseAuthorization();

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
