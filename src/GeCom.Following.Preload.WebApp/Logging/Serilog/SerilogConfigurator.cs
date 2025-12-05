using System.Globalization;
using Serilog;
using Serilog.Exceptions;

namespace GeCom.Following.Preload.WebApp.Logging.Serilog;

/// <summary>
/// Configures the Serilog logger.
/// </summary>
public static class SerilogConfigurator
{
    /// <summary>
    /// Initializes the logger with a bootstrap logger if it is not already initialized.
    /// </summary>
    public static void InitializeBootstrapLogger()
    {
        Log.Logger ??= new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();  // Bootstrap Logger for early initialization.

    }

    /// <summary>
    /// Configures the logger with the provided configuration in the logger.json configuration file.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public static void ConfigureLogger(IConfiguration configuration)
    {
        string? applicationName = configuration["ApplicationName"];

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId() // This requires the Serilog.Enrichers.CorrelationId package.
            .Enrich.WithExceptionDetails() // This requires the Serilog.Exceptions package.
            .Enrich.WithMachineName() // This requires the Serilog.Enrichers.Environment package.
            .Enrich.WithProcessId() // This requires the Serilog.Enrichers.Process package.
            .Enrich.WithThreadId() // This requires the Serilog.Enrichers.Thread package.
            .Enrich.WithProperty("ApplicationName", applicationName ?? "UnknownApp")
            .CreateLogger();
    }

    /// <summary>
    /// Adds Serilog to the host builder.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    public static void AddSerilogToHostBuilder(IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog();
    }
}

