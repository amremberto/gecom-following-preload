using Microsoft.AspNetCore.Authorization;

namespace GeCom.Following.Preload.WebApi.Extensions.Authorization;

/// <summary>
/// Extension methods for configuring authorization policies.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds authorization policies based on roles and permissions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPreloadAuthorization(this IServiceCollection services)
    {
        // Register the custom authorization handler
        services.AddSingleton<IAuthorizationHandler, PermissionOrRoleHandler>();

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

        // Policy: Require Manager or Administrator role
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireManagerOrAdministrator,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(
                    AuthorizationConstants.Roles.Manager,
                    AuthorizationConstants.Roles.Administrator));

        // Policy: Require societies read permission OR specific roles
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireSocietiesRead,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionOrRoleRequirement(
                    AuthorizationConstants.PermissionClaimType,
                    [
                        AuthorizationConstants.Permissions.SocietiesRead,
                        AuthorizationConstants.Permissions.PreloadRead,
                        AuthorizationConstants.Permissions.PreloadManage
                    ],
                    [
                        AuthorizationConstants.Roles.Administrator,
                        AuthorizationConstants.Roles.Manager,
                        AuthorizationConstants.Roles.User,
                        AuthorizationConstants.Roles.Viewer
                    ])));

        // Policy: Require societies create permission OR specific roles
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireSocietiesCreate,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionOrRoleRequirement(
                    AuthorizationConstants.PermissionClaimType,
                    [
                        AuthorizationConstants.Permissions.SocietiesCreate,
                        AuthorizationConstants.Permissions.PreloadManage
                    ],
                    [
                        AuthorizationConstants.Roles.Administrator,
                        AuthorizationConstants.Roles.Manager
                    ])));

        // Policy: Require societies update permission OR specific roles
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireSocietiesUpdate,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionOrRoleRequirement(
                    AuthorizationConstants.PermissionClaimType,
                    [
                        AuthorizationConstants.Permissions.SocietiesUpdate,
                        AuthorizationConstants.Permissions.PreloadManage
                    ],
                    [
                        AuthorizationConstants.Roles.Administrator,
                        AuthorizationConstants.Roles.Manager
                    ])));

        // Policy: Require societies delete permission OR Administrator role
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequireSocietiesDelete,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionOrRoleRequirement(
                    AuthorizationConstants.PermissionClaimType,
                    [
                        AuthorizationConstants.Permissions.SocietiesDelete,
                        AuthorizationConstants.Permissions.PreloadManage
                    ],
                    [
                        AuthorizationConstants.Roles.Administrator
                    ])));

        // Policy: Require preload read permission OR specific roles
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequirePreloadRead,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionOrRoleRequirement(
                    AuthorizationConstants.PermissionClaimType,
                    [
                        AuthorizationConstants.Permissions.PreloadRead,
                        AuthorizationConstants.Permissions.PreloadManage
                    ],
                    [
                        AuthorizationConstants.Roles.Administrator,
                        AuthorizationConstants.Roles.Manager,
                        AuthorizationConstants.Roles.User,
                        AuthorizationConstants.Roles.Viewer
                    ])));

        // Policy: Require preload manage permission OR specific roles
        authorizationBuilder.AddPolicy(
            AuthorizationConstants.Policies.RequirePreloadManage,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionOrRoleRequirement(
                    AuthorizationConstants.PermissionClaimType,
                    [
                        AuthorizationConstants.Permissions.PreloadManage
                    ],
                    [
                        AuthorizationConstants.Roles.Administrator,
                        AuthorizationConstants.Roles.Manager
                    ])));

        return services;
    }
}

