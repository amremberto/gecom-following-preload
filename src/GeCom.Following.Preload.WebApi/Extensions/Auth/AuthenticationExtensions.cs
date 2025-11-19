using System.Security.Claims;
using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

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
        // Note: The section name in JSON is "Authentication", not "AuthenticationSettings"
        services.Configure<AuthenticationSettings>(configuration.GetSection("Authentication"));

        AuthenticationSettings authenticationSettings = configuration
            .GetSection("Authentication")
            .Get<AuthenticationSettings>() ?? new();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                string authority = authenticationSettings.Authority.TrimEnd('/');

                options.Authority = authority; // IdentityServer URL
                options.Audience = authenticationSettings.Audience; // API resource name
                options.RequireHttpsMetadata = authenticationSettings.RequireHttpsMetadata; // true in production environments

                // Configure token validation parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidIssuer = authority,
                    RoleClaimType = AuthorizationConstants.RoleClaimType,
                    NameClaimType = "name"
                };

                // Configure events to map claims correctly
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Error(context.Exception, "Authentication failed. Path: {Path}", context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Ensure roles and email are mapped correctly
                        if (context.Principal is not null)
                        {
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity is not null)
                            {
                                // 1) Normalize ROLES
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

                                // 2) Normalize EMAIL
                                // Ensure email claim is available in both standard formats
                                string? email = identity.FindFirst("email")?.Value ??
                                              identity.FindFirst(ClaimTypes.Email)?.Value;

                                if (!string.IsNullOrWhiteSpace(email))
                                {
                                    // Ensure "email" claim exists
                                    if (!identity.HasClaim("email", email))
                                    {
                                        identity.AddClaim(new Claim("email", email));
                                    }

                                    // Ensure ClaimTypes.Email exists
                                    if (!identity.HasClaim(ClaimTypes.Email, email))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Email, email));
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

