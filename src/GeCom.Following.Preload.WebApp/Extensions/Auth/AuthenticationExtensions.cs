using System.Security.Claims;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace GeCom.Following.Preload.WebApp.Extensions.Auth;

/// <summary>
/// Extension methods for configuring OpenID Connect authentication.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds OpenID Connect authentication configured for IdentityServer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPreloadAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind strongly-typed options
        services.Configure<IdentityServerSettings>(configuration.GetSection("IdentityServer"));

        IdentityServerSettings identityServerSettings = configuration
            .GetSection("IdentityServer")
            .Get<IdentityServerSettings>() ?? new();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "GeCom.Following.Preload.WebApp.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.LoginPath = "/";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/unauthorized";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = identityServerSettings.Authority;
                options.ClientId = identityServerSettings.ClientId;

                // Configure client secret if provided
                if (!string.IsNullOrWhiteSpace(identityServerSettings.ClientSecret))
                {
                    options.ClientSecret = identityServerSettings.ClientSecret;
                }

                options.RequireHttpsMetadata = identityServerSettings.RequireHttpsMetadata;
                options.ResponseType = identityServerSettings.ResponseType;
                options.SaveTokens = identityServerSettings.SaveTokens;

                // Configure scopes
                foreach (string scope in identityServerSettings.RequiredScopes)
                {
                    options.Scope.Add(scope);
                }

                // Configure redirect URIs
                if (!string.IsNullOrWhiteSpace(identityServerSettings.RedirectUri))
                {
                    options.CallbackPath = new PathString("/signin-oidc");
                }

                if (!string.IsNullOrWhiteSpace(identityServerSettings.PostLogoutRedirectUri))
                {
                    options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                }

                // Configure PKCE
                if (identityServerSettings.UsePkce)
                {
                    options.UsePkce = true;
                }

                // Configure claim mapping
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    RoleClaimType = AuthorizationConstants.RoleClaimType,
                    NameClaimType = "name"
                };

                // Configure events to map claims correctly and handle token refresh
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Ensure roles and permissions are mapped correctly
                        // IdentityServer may send roles in different claim types
                        if (context.Principal is not null)
                        {
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity is not null)
                            {
                                // Map roles from various claim types to "role" claim
                                string[] roleClaimTypes =
                                [
                                    "role",
                                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                    ClaimTypes.Role
                                ];

                                foreach (string roleClaimType in roleClaimTypes)
                                {
                                    Claim[] roleClaims = [.. identity.FindAll(roleClaimType)];
                                    foreach (Claim roleClaim in roleClaims)
                                    {
                                        if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleClaim.Value))
                                        {
                                            identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleClaim.Value));
                                        }
                                    }
                                }

                                // Map permissions from various claim types to "permission" claim
                                string[] permissionClaimTypes =
                                [
                                    "permission",
                                    "scope",
                                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/permission"
                                ];

                                foreach (string permissionClaimType in permissionClaimTypes)
                                {
                                    Claim[] permissionClaims = [.. identity.FindAll(permissionClaimType)];
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
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Log the error for debugging
                        Exception? exception = context.Exception;
                        if (exception is not null)
                        {
                            // Log the error (you can use ILogger here if needed)
                            System.Diagnostics.Debug.WriteLine($"Authentication failed: {exception.Message}");
                        }

                        // If authentication fails, don't handle the response automatically
                        // Let the middleware handle it or redirect to error page
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        // Handle remote failures (e.g., invalid_client, invalid_grant, etc.)
                        Exception? exception = context.Failure;
                        if (exception is not null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Remote authentication failure: {exception.Message}");

                            // If it's an invalid_client error, it means the client is not configured in IdentityServer
                            if (exception.Message.Contains("invalid_client", StringComparison.OrdinalIgnoreCase))
                            {
                                context.HandleResponse();
                                context.Response.Redirect("/unauthorized?error=invalid_client");
                                return Task.CompletedTask;
                            }
                        }

                        context.HandleResponse();
                        context.Response.Redirect("/unauthorized?error=authentication_failed");
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        // Ensure the redirect URI is set correctly
                        if (!string.IsNullOrWhiteSpace(identityServerSettings.RedirectUri))
                        {
                            context.ProtocolMessage.RedirectUri = identityServerSettings.RedirectUri;
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        // Ensure the post-logout redirect URI is set correctly
                        if (!string.IsNullOrWhiteSpace(identityServerSettings.PostLogoutRedirectUri))
                        {
                            context.ProtocolMessage.PostLogoutRedirectUri = identityServerSettings.PostLogoutRedirectUri;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}

