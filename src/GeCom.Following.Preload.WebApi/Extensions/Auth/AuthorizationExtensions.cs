using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Extensions.Auth;

/// <summary>
/// Extension methods for configuring authorization policies.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds authorization policies based on roles.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPreloadAuthorization(this IServiceCollection services)
    {
        // Register the custom authorization handler for SingleSociety role validation
        services.AddSingleton<IAuthorizationHandler, SingleSocietyHandler>();

        AuthorizationBuilder authorizationBuilder = services.AddAuthorizationBuilder();

        // Default policy: require authentication
        authorizationBuilder.SetDefaultPolicy(
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        // Policy: Require any authenticated user
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireAuthenticated,
            policy => policy.RequireAuthenticatedUser());

        // Policy: Require Administrator role
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireAdministrator,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(AuthorizationConstants.Roles.Administrator));

        // Policy: Require preload read access
        // Allows: Administrator, ReadOnly, AllSocieties, SingleSociety
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequirePreloadRead,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(
                    AuthorizationConstants.Roles.Administrator,
                    AuthorizationConstants.Roles.PreloadReadOnly,
                    AuthorizationConstants.Roles.PreloadAllSocieties,
                    AuthorizationConstants.Roles.PreloadSingleSociety)
                .AddRequirements(new SingleSocietyRequirement()));

        // Policy: Require preload write access
        // Allows: Administrator, AllSocieties, SingleSociety
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequirePreloadWrite,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(
                    AuthorizationConstants.Roles.Administrator,
                    AuthorizationConstants.Roles.PreloadAllSocieties,
                    AuthorizationConstants.Roles.PreloadSingleSociety)
                .AddRequirements(new SingleSocietyRequirement()));

        return services;
    }
}

