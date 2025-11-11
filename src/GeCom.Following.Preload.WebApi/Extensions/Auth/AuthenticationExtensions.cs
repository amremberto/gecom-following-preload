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
                // Note: When using Authority, the middleware automatically gets the issuer from the discovery endpoint
                // We need to ensure the issuer validation works correctly
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Temporarily disable issuer validation to test if that's the issue
                    // Re-enable issuer validation once the issue is resolved
                    ValidateIssuer = true, // Temporarily disabled for debugging
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    // Explicitly set valid issuer to match the Authority
                    // This ensures tokens with issuer matching the Authority are accepted
                    ValidIssuer = authority,
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
                    OnChallenge = context =>
                    {
                        // Log challenge details for debugging
                        Log.Warning("Authentication challenge. Error: {Error}, ErrorDescription: {ErrorDescription}, Path: {Path}",
                            context.Error, context.ErrorDescription, context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures for debugging
                        Log.Error(context.Exception, "Authentication failed. Exception type: {ExceptionType}, Path: {Path}",
                            context.Exception.GetType().Name, context.Request.Path);

                        if (context.Exception is SecurityTokenInvalidIssuerException invalidIssuerEx)
                        {
                            // Log the actual issuer from the token if available
                            Log.Warning("Token issuer validation failed. Expected Authority: {Authority}, Exception: {Exception}",
                                authority, invalidIssuerEx.Message);

                            // Try to extract issuer from the token if available
                            if (context.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
                            {
                                string? authHeader = value.ToString();
                                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                                {
                                    string token = authHeader.Substring("Bearer ".Length).Trim();
                                    try
                                    {
                                        // Decode JWT to get issuer (without validation)
                                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                                        System.IdentityModel.Tokens.Jwt.JwtSecurityToken? jsonToken = handler.ReadJwtToken(token);
                                        Log.Warning("Token issuer: {TokenIssuer}, Expected Authority: {Authority}, Matches: {Matches}",
                                            jsonToken.Issuer, authority,
                                            jsonToken.Issuer.Equals(authority, StringComparison.OrdinalIgnoreCase) ||
                                            jsonToken.Issuer.Equals($"{authority}/", StringComparison.OrdinalIgnoreCase));
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Warning(ex, "Error decoding token");
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning("No Authorization header found in request");
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        // Log when token is received for debugging
                        Log.Information("JWT Bearer authentication message received. Path: {Path}", context.Request.Path);

                        if (context.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
                        {
                            string? authHeader = value.ToString();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                string token = authHeader.Substring("Bearer ".Length).Trim();
                                try
                                {
                                    // Decode JWT to get issuer (without validation) for debugging
                                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                                    System.IdentityModel.Tokens.Jwt.JwtSecurityToken? jsonToken = handler.ReadJwtToken(token);
                                    Log.Information("Token received. Issuer: {TokenIssuer}, Authority: {Authority}, Audience: {Audience}",
                                        jsonToken.Issuer, authority, jsonToken.Audiences.FirstOrDefault());
                                }
                                catch (Exception ex)
                                {
                                    Log.Warning(ex, "Error decoding token in OnMessageReceived");
                                }
                            }
                            else
                            {
                                Log.Warning("Authorization header found but does not start with 'Bearer'. Header: {Header}",
                                    authHeader?.Substring(0, Math.Min(50, authHeader.Length)));
                            }
                        }
                        else
                        {
                            Log.Warning("No Authorization header found in request. Path: {Path}", context.Request.Path);
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Log successful token validation
                        Log.Information("Token validated successfully. Subject: {Subject}, Path: {Path}",
                            context.Principal?.Identity?.Name ?? "Unknown", context.Request.Path);

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

