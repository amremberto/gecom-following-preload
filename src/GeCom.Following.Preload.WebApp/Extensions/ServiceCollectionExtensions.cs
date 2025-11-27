using GeCom.Following.Preload.WebApp.Configurations.Settings;
using GeCom.Following.Preload.WebApp.Services;
using GeCom.Following.Preload.WebApp.Services.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Extensions;

/// <summary>
/// Extension methods for registering application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Get API settings
        PreloadApiSettings apiSettings = configuration
            .GetSection("PreloadApi")
            .Get<PreloadApiSettings>() ?? new();

        // Register the authentication delegating handler as transient
        services.AddTransient<AuthenticationDelegatingHandler>();

        // Register HttpClient for HTTP client service with authentication handler
        services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
        {
            if (!string.IsNullOrWhiteSpace(apiSettings.BaseUrl))
            {
                client.BaseAddress = new Uri(apiSettings.BaseUrl);
            }

            // Increased timeout for file downloads (PDFs can be large)
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        // Register dashboard service
        services.AddScoped<IDashboardService, DashboardService>();

        // Register provider service
        services.AddScoped<IProviderService, ProviderService>();

        // Register document service
        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}

