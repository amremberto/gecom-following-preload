using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.AspNetCore.Authentication;
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
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
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

                // Enable UserInfo endpoint to get additional claims
                options.GetClaimsFromUserInfoEndpoint = true;

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
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal?.Identity is not ClaimsIdentity identity)
                        {
                            return;
                        }

                        // 1) Normalizar NAME
                        string? name =
                            identity.FindFirst("name")?.Value
                            ?? identity.FindFirst("preferred_username")?.Value
                            ?? BuildNameFromGivenAndFamily(identity)
                            ?? identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = "Usuario";
                        }

                        // Garantizar "name"
                        Claim? existingNameClaim = identity.FindFirst(c => c.Type == "name");
                        if (existingNameClaim is not null && existingNameClaim.Value != name)
                        {
                            identity.RemoveClaim(existingNameClaim);
                            identity.AddClaim(new Claim("name", name));
                        }
                        else if (existingNameClaim is null)
                        {
                            identity.AddClaim(new Claim("name", name));
                        }

                        // Garantizar ClaimTypes.Name
                        Claim? existingClaimTypesName = identity.FindFirst(c => c.Type == ClaimTypes.Name);
                        if (existingClaimTypesName is not null && existingClaimTypesName.Value != name)
                        {
                            identity.RemoveClaim(existingClaimTypesName);
                            identity.AddClaim(new Claim(ClaimTypes.Name, name));
                        }
                        else if (existingClaimTypesName is null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, name));
                        }

                        // 2) Normalizar ROLES
                        var roleValues = new HashSet<string>(StringComparer.Ordinal);

                        // 2.1. Recoger roles ya presentes en la identidad (cualquier tipo de role conocido)
                        foreach (Claim claim in identity.Claims)
                        {
                            if ((claim.Type == AuthorizationConstants.RoleClaimType ||
                                claim.Type == ClaimTypes.Role ||
                                claim.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase) ||
                                claim.Type.EndsWith("/claims/role", StringComparison.OrdinalIgnoreCase)) &&
                                !string.IsNullOrWhiteSpace(claim.Value))
                            {
                                roleValues.Add(claim.Value);
                            }
                        }

                        // 2.2. Extraer roles desde el access_token
                        string? accessToken =
                            context.Properties?.GetTokenValue("access_token")
                            ?? context.TokenEndpointResponse?.AccessToken;

                        if (!string.IsNullOrWhiteSpace(accessToken))
                        {
                            int rolesAddedFromToken = ExtractRolesFromAccessToken(accessToken, identity);
                            System.Diagnostics.Debug.WriteLine($"OnTokenValidated: roles añadidos desde access_token: {rolesAddedFromToken}");

                            // Añadimos esos valores al HashSet también
                            Claim[] roleClaimsFromToken = [.. identity.FindAll(AuthorizationConstants.RoleClaimType)];
                            foreach (Claim roleClaim in roleClaimsFromToken)
                            {
                                if (!string.IsNullOrWhiteSpace(roleClaim.Value))
                                {
                                    roleValues.Add(roleClaim.Value);
                                }
                            }

                            Claim[] roleClaimsFromToken2 = [.. identity.FindAll(ClaimTypes.Role)];
                            foreach (Claim roleClaim in roleClaimsFromToken2)
                            {
                                if (!string.IsNullOrWhiteSpace(roleClaim.Value))
                                {
                                    roleValues.Add(roleClaim.Value);
                                }
                            }
                        }

                        // 2.3. Limpiar y volver a crear claims de rol de forma consistente
                        var claimsToRemove = identity.Claims
                            .Where(c => c.Type == AuthorizationConstants.RoleClaimType || c.Type == ClaimTypes.Role)
                            .ToList();

                        foreach (Claim claim in claimsToRemove)
                        {
                            identity.RemoveClaim(claim);
                        }

                        foreach (string role in roleValues)
                        {
                            identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, role));
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }

                        // 3) Reconstruir Principal con NameClaimType y RoleClaimType correctos
                        var newIdentity = new ClaimsIdentity(
                            identity.Claims,
                            identity.AuthenticationType,
                            "name",
                            AuthorizationConstants.RoleClaimType);

                        var newPrincipal = new ClaimsPrincipal(newIdentity);
                        context.Principal = newPrincipal;

                        await Task.CompletedTask;
                    },
                    OnUserInformationReceived = context =>
                    {
                        // This event is called after the UserInfo endpoint is called
                        // The claims from UserInfo endpoint are automatically added to context.Principal
                        // when GetClaimsFromUserInfoEndpoint is true, so we just need to map them
                        if (context.Principal is not null)
                        {
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity is not null)
                            {
                                // Map the name again with the new claims from UserInfo
                                string? userName = null;
                                Claim[] allNameClaims = [.. identity.FindAll("name")];

                                // Prioritize name claim from OpenIdConnect issuer
                                Claim? openIdConnectNameClaim = allNameClaims.FirstOrDefault(c =>
                                    c.Issuer.Contains("OpenIdConnect", StringComparison.OrdinalIgnoreCase) ||
                                    c.Issuer.Contains("localhost:7100", StringComparison.OrdinalIgnoreCase) ||
                                    c.Issuer.Contains("IdentityServer", StringComparison.OrdinalIgnoreCase) ||
                                    !c.Issuer.Contains("LOCAL AUTHORITY", StringComparison.OrdinalIgnoreCase) &&
                                    !c.Value.StartsWith("Usuario ", StringComparison.OrdinalIgnoreCase) &&
                                    !string.IsNullOrWhiteSpace(c.Value));

                                if (openIdConnectNameClaim is not null && !string.IsNullOrWhiteSpace(openIdConnectNameClaim.Value))
                                {
                                    userName = openIdConnectNameClaim.Value;
                                }

                                // If no name from OpenIdConnect, try preferred_username
                                if (string.IsNullOrWhiteSpace(userName))
                                {
                                    Claim? preferredUsernameClaim = identity.FindFirst("preferred_username");
                                    if (preferredUsernameClaim is not null && !string.IsNullOrWhiteSpace(preferredUsernameClaim.Value))
                                    {
                                        userName = preferredUsernameClaim.Value;
                                    }
                                }

                                // If no name found, try combining given_name and family_name
                                if (string.IsNullOrWhiteSpace(userName))
                                {
                                    string? givenName = identity.FindFirst("given_name")?.Value;
                                    string? familyName = identity.FindFirst("family_name")?.Value;
                                    if (!string.IsNullOrWhiteSpace(givenName) || !string.IsNullOrWhiteSpace(familyName))
                                    {
                                        userName = $"{givenName} {familyName}".Trim();
                                    }
                                }

                                // If we found a name, ensure it's set as the "name" and ClaimTypes.Name claims
                                if (!string.IsNullOrWhiteSpace(userName))
                                {
                                    // Remove existing "name" claims that are NOT from OpenIdConnect (fallbacks)
                                    Claim[] existingNameClaims = [.. identity.FindAll("name")];
                                    foreach (Claim existingClaim in existingNameClaims)
                                    {
                                        if (existingClaim.Issuer.Contains("LOCAL AUTHORITY", StringComparison.OrdinalIgnoreCase) ||
                                            existingClaim.Value.StartsWith("Usuario ", StringComparison.OrdinalIgnoreCase))
                                        {
                                            identity.RemoveClaim(existingClaim);
                                        }
                                    }

                                    // Only add if we don't already have a valid name from OpenIdConnect
                                    if (openIdConnectNameClaim is null || openIdConnectNameClaim.Value != userName)
                                    {
                                        identity.AddClaim(new Claim("name", userName));
                                    }

                                    // Always ensure ClaimTypes.Name is set for Blazor compatibility
                                    Claim[] existingClaimTypesName = [.. identity.FindAll(ClaimTypes.Name)];
                                    foreach (Claim existingClaim in existingClaimTypesName)
                                    {
                                        identity.RemoveClaim(existingClaim);
                                    }
                                    identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                                }

                                // Map roles from UserInfo endpoint
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

                                // Ensure roles are also added as ClaimTypes.Role for IsInRole() to work
                                Claim[] allRoleClaimsFromUserInfo = [.. identity.FindAll(AuthorizationConstants.RoleClaimType)];
                                foreach (Claim roleClaim in allRoleClaimsFromUserInfo)
                                {
                                    if (!identity.HasClaim(ClaimTypes.Role, roleClaim.Value))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                                    }
                                }

                                // Replace the Principal to ensure all claims are saved to the cookie
                                var newIdentityFromUserInfo = new ClaimsIdentity(
                                    identity.Claims,
                                    identity.AuthenticationType,
                                    identity.NameClaimType ?? "name",
                                    AuthorizationConstants.RoleClaimType);
                                var newPrincipalFromUserInfo = new ClaimsPrincipal(newIdentityFromUserInfo);
                                context.Principal = newPrincipalFromUserInfo;
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context
                        => Task.CompletedTask,
                    OnRemoteFailure = context =>
                    {
                        // Handle remote failures (e.g., invalid_client, invalid_grant, etc.)
                        Exception? exception = context.Failure;
                        if (exception is not null && exception.Message.Contains("invalid_client", StringComparison.OrdinalIgnoreCase))
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/unauthorized?error=invalid_client");
                            return Task.CompletedTask;
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

                        // Request additional claims explicitly
                        // This ensures IdentityServer includes name, email, profile, and role claims
                        context.ProtocolMessage.SetParameter("claims", "{\"id_token\":{\"name\":null,\"email\":null,\"given_name\":null,\"family_name\":null,\"preferred_username\":null,\"role\":null},\"userinfo\":{\"name\":null,\"email\":null,\"given_name\":null,\"family_name\":null,\"preferred_username\":null,\"role\":null}}");

                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        // Ensure the post-logout redirect URI is set correctly
                        if (!string.IsNullOrWhiteSpace(identityServerSettings.PostLogoutRedirectUri))
                        {
                            context.ProtocolMessage.PostLogoutRedirectUri = identityServerSettings.PostLogoutRedirectUri;
                        }

                        // Get the id_token from multiple sources to ensure we have it
                        string? idToken = null;

                        // First, try to get it from the authentication properties
                        if (context.Properties.Items.TryGetValue("id_token", out string? idTokenFromItems))
                        {
                            idToken = idTokenFromItems;
                        }

                        // If not in items, try to get it from the token store
                        if (string.IsNullOrWhiteSpace(idToken))
                        {
                            idToken = context.Properties.GetTokenValue("id_token");
                        }

                        // If still not found, try to get it from HttpContext
                        if (string.IsNullOrWhiteSpace(idToken))
                        {
                            try
                            {
                                idToken = context.HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "id_token").GetAwaiter().GetResult();
                            }
                            catch
                            {
                                // Token not available, continue without it
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(idToken))
                        {
                            context.ProtocolMessage.IdTokenHint = idToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnSignedOutCallbackRedirect = async context =>
                    {
                        // Sign out from cookie scheme to clear all authentication cookies
                        // This ensures the user is fully logged out from the application
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                        // Delete the main authentication cookie
                        context.Response.Cookies.Delete("GeCom.Following.Preload.WebApp.Auth", new CookieOptions
                        {
                            Path = "/",
                            HttpOnly = true,
                            Secure = context.Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });

                        // Clean up fragmented cookies (if cookie was too large and got fragmented)
                        context.Response.Cookies.Delete("GeCom.Following.Preload.WebApp.AuthC1", new CookieOptions
                        {
                            Path = "/",
                            HttpOnly = true,
                            Secure = context.Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });

                        context.Response.Cookies.Delete("GeCom.Following.Preload.WebApp.AuthC2", new CookieOptions
                        {
                            Path = "/",
                            HttpOnly = true,
                            Secure = context.Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });
                    }
                };
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
                options.AccessDeniedPath = "/unauthorized";
            });

        return services;
    }

    /// <summary>
    /// Helper to build a display name from given_name and family_name.
    /// </summary>
    private static string? BuildNameFromGivenAndFamily(ClaimsIdentity identity)
    {
        string? givenName = identity.FindFirst("given_name")?.Value;
        string? familyName = identity.FindFirst("family_name")?.Value;

        if (!string.IsNullOrWhiteSpace(givenName) || !string.IsNullOrWhiteSpace(familyName))
        {
            return $"{givenName} {familyName}".Trim();
        }

        return null;
    }

    /// <summary>
    /// Extracts roles from the access_token JWT and adds them to the ClaimsIdentity.
    /// </summary>
    /// <param name="accessToken">The access token JWT string.</param>
    /// <param name="identity">The ClaimsIdentity to add roles to.</param>
    /// <returns>The number of roles added.</returns>
    private static int ExtractRolesFromAccessToken(string accessToken, ClaimsIdentity identity)
    {
        int rolesAdded = 0;

        try
        {
            // JWT format: header.payload.signature
            string[] parts = accessToken.Split('.');
            if (parts.Length < 2)
            {
                return 0;
            }

            // Decode the payload (second part)
            string payloadBase64 = parts[1];

            // Add padding if necessary (Base64URL encoding may omit padding)
            switch (payloadBase64.Length % 4)
            {
                case 2:
                    payloadBase64 += "==";
                    break;
                case 3:
                    payloadBase64 += "=";
                    break;
            }

            // Replace Base64URL characters with Base64 characters
            payloadBase64 = payloadBase64.Replace('-', '+').Replace('_', '/');

            // Decode from Base64
            byte[] payloadBytes = Convert.FromBase64String(payloadBase64);
            string payloadJson = Encoding.UTF8.GetString(payloadBytes);

            // Parse JSON
            using var doc = JsonDocument.Parse(payloadJson);
            JsonElement root = doc.RootElement;

            // Extract roles from the "role" claim
            // Roles can be a single string or an array of strings
            if (root.TryGetProperty("role", out JsonElement roleElement))
            {
                if (roleElement.ValueKind == JsonValueKind.Array)
                {
                    // Roles are in an array
                    foreach (JsonElement roleItem in roleElement.EnumerateArray())
                    {
                        string? roleValue = roleItem.GetString();
                        if (!string.IsNullOrWhiteSpace(roleValue))
                        {
                            // Add as AuthorizationConstants.RoleClaimType
                            if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleValue))
                            {
                                identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleValue));
                                rolesAdded++;
                            }

                            // Also add as ClaimTypes.Role for IsInRole() to work
                            if (!identity.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
                else if (roleElement.ValueKind == JsonValueKind.String)
                {
                    // Single role as string
                    string? roleValue = roleElement.GetString();
                    if (!string.IsNullOrWhiteSpace(roleValue))
                    {
                        if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleValue))
                        {
                            identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleValue));
                            rolesAdded++;
                        }

                        if (!identity.HasClaim(ClaimTypes.Role, roleValue))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                        }
                    }
                }
            }
        }
        catch
        {
            // Error extracting roles from access token, return 0
        }

        return rolesAdded;
    }
}
