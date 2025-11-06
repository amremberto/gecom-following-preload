using System.Security.Claims;
using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GeCom.Following.Preload.WebApi.Extensions.Auth;

/// <summary>
/// Extension methods for configuring JWT authentication.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication configured for IdentityServer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPreloadAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind strongly-typed options
        services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));

        AuthenticationSettings authenticationSettings = configuration
            .GetSection(nameof(AuthenticationSettings))
            .Get<AuthenticationSettings>() ?? new();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authenticationSettings.Authority; // IdentityServer URL
                options.Audience = authenticationSettings.Audience; // API resource name
                options.RequireHttpsMetadata = authenticationSettings.RequireHttpsMetadata; // true in production environments

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    // Map roles from JWT claims
                    // IdentityServer typically uses "role" claim, but also supports "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    RoleClaimType = AuthorizationConstants.RoleClaimType,
                    // Map permissions from JWT claims
                    // IdentityServer can include permissions in "permission" claim or as scopes
                    NameClaimType = "name"
                };

                // Configure events to map claims correctly
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Ensure roles are mapped correctly
                        // IdentityServer may send roles in different claim types
                        if (context.Principal is not null)
                        {
                            // Map roles from various claim types to "role" claim
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity is not null)
                            {
                                // Check for roles in standard claim types
                                string[] roleClaimTypes = [
                                    "role",
                                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                    ClaimTypes.Role
                                ];

                                foreach (string roleClaimType in roleClaimTypes)
                                {
                                    Claim[] roleClaims = identity.FindAll(roleClaimType).ToArray();
                                    foreach (Claim roleClaim in roleClaims)
                                    {
                                        if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleClaim.Value))
                                        {
                                            identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleClaim.Value));
                                        }
                                    }
                                }

                                // Map permissions from various claim types to "permission" claim
                                string[] permissionClaimTypes = [
                                    "permission",
                                    "scope",
                                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/permission"
                                ];

                                foreach (string permissionClaimType in permissionClaimTypes)
                                {
                                    Claim[] permissionClaims = identity.FindAll(permissionClaimType).ToArray();
                                    foreach (Claim permissionClaim in permissionClaims)
                                    {
                                        if (!identity.HasClaim(AuthorizationConstants.PermissionClaimType, permissionClaim.Value))
                                        {
                                            identity.AddClaim(new Claim(AuthorizationConstants.PermissionClaimType, permissionClaim.Value));
                                        }
                                    }
                                }
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}

