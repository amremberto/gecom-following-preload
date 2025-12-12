using GeCom.Following.Preload.WebApp.Components;
using GeCom.Following.Preload.WebApp.Configurations;
using GeCom.Following.Preload.WebApp.Extensions;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Logging.Serilog;
using GeCom.Following.Preload.WebApp.Middlewares;
using GeCom.Following.Preload.WebApp.Services;
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

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Add authentication state provider for Blazor Server
    // This automatically configures the AuthenticationStateProvider needed for AuthorizeView
    builder.Services.AddCascadingAuthenticationState();

    // Add controllers for API endpoints (if needed)
    builder.Services.AddControllers();

    // Add HttpContextAccessor for accessing HttpContext in Blazor Server components
    builder.Services.AddHttpContextAccessor();

    // Habilitar DetailedErrors en desarrollo para ver excepciones detalladas
    //if (builder.Environment.IsDevelopment())
    builder.Services.AddServerSideBlazor().AddCircuitOptions(options => options.DetailedErrors = true);

    // Add OpenID Connect authentication services
    builder.Services.AddPreloadAuthentication(builder.Configuration);

    // Add authorization services with custom policies
    builder.Services.AddPreloadAuthorization();

    // Add API client service
    builder.Services.AddApiClient(builder.Configuration);

    // Add toast service
    builder.Services.AddScoped<IToastService, ToastService>();

    #endregion

    WebApplication? app = builder.Build();

    // Validate configuration at startup
    app.ValidateConfiguration();

    #region Configure the HTTP request pipeline.

    // Adds middleware for streamlined request logging.
    //app.UseSerilogRequestLogging()

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    //app.UseHttpsRedirection()

    app.UseAntiforgery();

    // Use authentication (must be before UseAuthorization)
    app.UseAuthentication();

    // Add Identity Server error handling middleware (after authentication to catch auth errors)
    app.UseIdentityServerErrorHandling();

    // Use authorization
    app.UseAuthorization();

    app.MapStaticAssets();

    // Map API controllers
    app.MapControllers();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

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
