using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using Microsoft.Extensions.Options;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace GeCom.Following.Preload.WebApi.Extensions.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Note: ApiSwaggerSettings and IdentityServerSettings are already registered
        // in AddConfigurationSettings, so we don't need to register them again here.

        using ServiceProvider provider = services.BuildServiceProvider();
        IOptions<ApiSwaggerSettings> swaggerOptions = provider.GetRequiredService<IOptions<ApiSwaggerSettings>>();
        ApiSwaggerSettings swaggerSettings = swaggerOptions.Value;
        IOptions<IdentityServerSettings> idsrvOptions = provider.GetRequiredService<IOptions<IdentityServerSettings>>();
        IdentityServerSettings identityServerSettings = idsrvOptions.Value;

        if (!swaggerSettings.Enabled)
        {
            return services; // Swagger disabled via configuration
        }

        services.AddOpenApiDocument(configure =>
        {
            configure.Title = swaggerSettings.Title;
            configure.Version = swaggerSettings.Version;
            configure.DocumentName = swaggerSettings.DocumentName;

            // Add OAuth2 security definition (Authorization Code + PKCE)
            if (swaggerSettings.EnableOAuth2Security)
            {
                configure.AddSecurity("OAuth2", [], new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Description = "OAuth2 Authorization Code Flow",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = identityServerSettings.Authority.TrimEnd('/') + "/connect/authorize",
                            TokenUrl = identityServerSettings.Authority.TrimEnd('/') + "/connect/token",
                            Scopes = (identityServerSettings.RequiredScopes ?? [])
                                .Distinct()
                                .ToDictionary(scope => scope, scope => "Access to " + scope)
                        }
                    }
                });
                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("OAuth2"));
            }
        });

        return services;
    }
}


