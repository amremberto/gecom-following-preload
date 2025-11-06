using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Security.Authorization.WebApi.Extensions.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddAdminApiCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Bind strongly-typed options
        services.Configure<CorsSettings>(configuration.GetSection("Cors"));

        services.AddCors(options =>
        {
            using ServiceProvider provider = services.BuildServiceProvider();
            IOptions<CorsSettings> corsOptions = provider.GetRequiredService<IOptions<CorsSettings>>();
            CorsSettings settings = corsOptions.Value;

            string resolvedPolicyName = string.IsNullOrWhiteSpace(settings.PolicyName)
                ? "DefaultCorsPolicy"
                : settings.PolicyName;

            options.AddPolicy(resolvedPolicyName, builder =>
            {
                if (environment.IsDevelopment())
                {
                    // Development: open CORS for speed of iteration
                    builder.AllowAnyOrigin();
                }
                else
                {
                    // Non-Development: require explicit origins
                    if (settings.AllowedOrigins.Length == 0)
                    {
                        throw new InvalidOperationException("CORS configuration error: 'AllowedOrigins' must be specified in non-development environments.");
                    }

                    builder.WithOrigins(settings.AllowedOrigins);

                    if (settings.AllowCredentials)
                    {
                        builder.AllowCredentials();
                    }
                }

                if (settings.AllowedMethods.Length > 0)
                {
                    builder.WithMethods(settings.AllowedMethods);
                }
                else
                {
                    builder.AllowAnyMethod();
                }

                if (settings.AllowedHeaders.Length > 0)
                {
                    builder.WithHeaders(settings.AllowedHeaders);
                }
                else
                {
                    builder.AllowAnyHeader();
                }

                if (settings.ExposedHeaders.Length > 0)
                {
                    builder.WithExposedHeaders(settings.ExposedHeaders);
                }
            });
        });

        return services;
    }
}
