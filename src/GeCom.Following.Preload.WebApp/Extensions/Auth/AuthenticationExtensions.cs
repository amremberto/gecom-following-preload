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
                    OnTokenValidated = context =>
                    {
                        // Ensure roles and permissions are mapped correctly
                        // IdentityServer may send roles in different claim types
                        if (context.Principal is not null)
                        {
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity is not null)
                            {
                                // Log all claims for debugging (remove in production)
                                System.Diagnostics.Debug.WriteLine("=== Claims from IdentityServer ===");
                                foreach (Claim claim in identity.Claims)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}, Issuer: {claim.Issuer}");
                                }
                                System.Diagnostics.Debug.WriteLine("=== End Claims ===");

                                // Map name from various claim types to "name" claim
                                // Priority: 1) name from OpenIdConnect, 2) preferred_username, 3) given_name + family_name, 4) fallback

                                // First, try to find the "name" claim from OpenIdConnect (IdentityServer)
                                // This is the most reliable source
                                string? userName = null;
                                Claim[] allNameClaims = [.. identity.FindAll("name")];

                                // Prioritize name claim from OpenIdConnect issuer
                                // The issuer from IdentityServer is typically "OpenIdConnect" or the Authority URL
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
                                    System.Diagnostics.Debug.WriteLine($"Found name claim from OpenIdConnect: {userName}");
                                }

                                // If no name from OpenIdConnect, try preferred_username
                                if (string.IsNullOrWhiteSpace(userName))
                                {
                                    Claim? preferredUsernameClaim = identity.FindFirst("preferred_username");
                                    if (preferredUsernameClaim is not null && !string.IsNullOrWhiteSpace(preferredUsernameClaim.Value))
                                    {
                                        userName = preferredUsernameClaim.Value;
                                        System.Diagnostics.Debug.WriteLine($"Found preferred_username claim: {userName}");
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
                                        System.Diagnostics.Debug.WriteLine($"Combined name from given_name and family_name: {userName}");
                                    }
                                }

                                // If still no name found, try to get it from nameidentifier as fallback
                                if (string.IsNullOrWhiteSpace(userName))
                                {
                                    Claim? nameIdentifierClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                                    if (nameIdentifierClaim is not null && !string.IsNullOrWhiteSpace(nameIdentifierClaim.Value))
                                    {
                                        // Use a formatted version of the nameidentifier as fallback
                                        // Extract a short identifier from the GUID
                                        string shortId = nameIdentifierClaim.Value.Length > 8
                                            ? nameIdentifierClaim.Value.Substring(0, 8).ToUpperInvariant()
                                            : nameIdentifierClaim.Value.ToUpperInvariant();
                                        userName = $"Usuario {shortId}";
                                        System.Diagnostics.Debug.WriteLine($"Using nameidentifier as fallback: {userName}");
                                    }
                                }

                                // Final fallback: use "Usuario" if nothing else is available
                                if (string.IsNullOrWhiteSpace(userName))
                                {
                                    userName = "Usuario";
                                    System.Diagnostics.Debug.WriteLine("Using default 'Usuario' as fallback");
                                }

                                // If we found a name, ensure it's set as the "name" and ClaimTypes.Name claims
                                // But only if we don't already have a valid name from OpenIdConnect
                                if (!string.IsNullOrWhiteSpace(userName))
                                {
                                    // Remove existing "name" claims that are NOT from OpenIdConnect (fallbacks)
                                    Claim[] existingNameClaims = [.. identity.FindAll("name")];
                                    foreach (Claim existingClaim in existingNameClaims)
                                    {
                                        // Only remove fallback claims, keep OpenIdConnect claims
                                        if (existingClaim.Issuer.Contains("LOCAL AUTHORITY", StringComparison.OrdinalIgnoreCase) ||
                                            existingClaim.Value.StartsWith("Usuario ", StringComparison.OrdinalIgnoreCase))
                                        {
                                            identity.RemoveClaim(existingClaim);
                                            System.Diagnostics.Debug.WriteLine($"Removed fallback name claim: {existingClaim.Value}");
                                        }
                                    }

                                    // Only add if we don't already have a valid name from OpenIdConnect
                                    if (openIdConnectNameClaim is null || openIdConnectNameClaim.Value != userName)
                                    {
                                        identity.AddClaim(new Claim("name", userName));
                                        System.Diagnostics.Debug.WriteLine($"Added 'name' claim: {userName}");
                                    }

                                    // Always ensure ClaimTypes.Name is set for Blazor compatibility
                                    Claim[] existingClaimTypesName = [.. identity.FindAll(ClaimTypes.Name)];
                                    foreach (Claim existingClaim in existingClaimTypesName)
                                    {
                                        identity.RemoveClaim(existingClaim);
                                    }
                                    identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                                    System.Diagnostics.Debug.WriteLine($"Added ClaimTypes.Name claim: {userName}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("WARNING: No name claim found in token!");
                                }

                                // Map roles from various claim types to "role" claim
                                System.Diagnostics.Debug.WriteLine("=== Mapping roles from token ===");
                                string[] roleClaimTypes =
                                [
                                    "role",
                                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                    ClaimTypes.Role
                                ];

                                int rolesAdded = 0;
                                foreach (string roleClaimType in roleClaimTypes)
                                {
                                    Claim[] roleClaims = [.. identity.FindAll(roleClaimType)];
                                    System.Diagnostics.Debug.WriteLine($"Found {roleClaims.Length} claims of type '{roleClaimType}'");
                                    foreach (Claim roleClaim in roleClaims)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  - Role claim: Type={roleClaim.Type}, Value={roleClaim.Value}, Issuer={roleClaim.Issuer}");
                                        if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleClaim.Value))
                                        {
                                            identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleClaim.Value));
                                            System.Diagnostics.Debug.WriteLine($"Added 'role' claim from token: {roleClaim.Value}");
                                            rolesAdded++;
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Role claim '{roleClaim.Value}' already exists, skipping");
                                        }
                                    }
                                }
                                System.Diagnostics.Debug.WriteLine($"=== Total roles added from token: {rolesAdded} ===");

                                // Extract roles from access_token
                                // The access_token contains roles but is not automatically processed by OpenIdConnect middleware
                                if (context.Properties is not null)
                                {
                                    string? accessToken = context.Properties.GetTokenValue("access_token");
                                    if (!string.IsNullOrWhiteSpace(accessToken))
                                    {
                                        System.Diagnostics.Debug.WriteLine("=== Extracting roles from access_token ===");
                                        int accessTokenRolesAdded = ExtractRolesFromAccessToken(accessToken, identity);
                                        System.Diagnostics.Debug.WriteLine($"=== Total roles added from access_token: {accessTokenRolesAdded} ===");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("WARNING: access_token not found in authentication properties");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("WARNING: Authentication properties are null");
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
                                // Log all claims after UserInfo endpoint call
                                System.Diagnostics.Debug.WriteLine("=== Claims after UserInfo Endpoint ===");
                                foreach (Claim claim in identity.Claims)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}, Issuer: {claim.Issuer}");
                                }
                                System.Diagnostics.Debug.WriteLine("=== End Claims ===");

                                // Now try to map the name again with the new claims from UserInfo
                                // Use the same priority logic as OnTokenValidated

                                // First, try to find the "name" claim from OpenIdConnect (IdentityServer)
                                string? userName = null;
                                Claim[] allNameClaims = [.. identity.FindAll("name")];

                                // Prioritize name claim from OpenIdConnect issuer
                                // The issuer from IdentityServer is typically "OpenIdConnect" or the Authority URL
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
                                    System.Diagnostics.Debug.WriteLine($"Found name claim from OpenIdConnect (UserInfo): {userName}");
                                }

                                // If no name from OpenIdConnect, try preferred_username
                                if (string.IsNullOrWhiteSpace(userName))
                                {
                                    Claim? preferredUsernameClaim = identity.FindFirst("preferred_username");
                                    if (preferredUsernameClaim is not null && !string.IsNullOrWhiteSpace(preferredUsernameClaim.Value))
                                    {
                                        userName = preferredUsernameClaim.Value;
                                        System.Diagnostics.Debug.WriteLine($"Found preferred_username claim from UserInfo: {userName}");
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
                                        System.Diagnostics.Debug.WriteLine($"Combined name from UserInfo: {userName}");
                                    }
                                }

                                // If we found a name, ensure it's set as the "name" and ClaimTypes.Name claims
                                // But only if we don't already have a valid name from OpenIdConnect
                                if (!string.IsNullOrWhiteSpace(userName))
                                {
                                    // Remove existing "name" claims that are NOT from OpenIdConnect (fallbacks)
                                    Claim[] existingNameClaims = [.. identity.FindAll("name")];
                                    foreach (Claim existingClaim in existingNameClaims)
                                    {
                                        // Only remove fallback claims, keep OpenIdConnect claims
                                        if (existingClaim.Issuer.Contains("LOCAL AUTHORITY", StringComparison.OrdinalIgnoreCase) ||
                                            existingClaim.Value.StartsWith("Usuario ", StringComparison.OrdinalIgnoreCase))
                                        {
                                            identity.RemoveClaim(existingClaim);
                                            System.Diagnostics.Debug.WriteLine($"Removed fallback name claim from UserInfo: {existingClaim.Value}");
                                        }
                                    }

                                    // Only add if we don't already have a valid name from OpenIdConnect
                                    if (openIdConnectNameClaim is null || openIdConnectNameClaim.Value != userName)
                                    {
                                        identity.AddClaim(new Claim("name", userName));
                                        System.Diagnostics.Debug.WriteLine($"Added 'name' claim from UserInfo: {userName}");
                                    }

                                    // Always ensure ClaimTypes.Name is set for Blazor compatibility
                                    Claim[] existingClaimTypesName = [.. identity.FindAll(ClaimTypes.Name)];
                                    foreach (Claim existingClaim in existingClaimTypesName)
                                    {
                                        identity.RemoveClaim(existingClaim);
                                    }
                                    identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                                    System.Diagnostics.Debug.WriteLine($"Added ClaimTypes.Name claim from UserInfo: {userName}");
                                }

                                // Map roles from UserInfo endpoint
                                // IdentityServer may send roles in different claim types
                                System.Diagnostics.Debug.WriteLine("=== Mapping roles from UserInfo endpoint ===");
                                string[] roleClaimTypes =
                                [
                                    "role",
                                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                    ClaimTypes.Role
                                ];

                                int rolesAdded = 0;
                                foreach (string roleClaimType in roleClaimTypes)
                                {
                                    Claim[] roleClaims = [.. identity.FindAll(roleClaimType)];
                                    System.Diagnostics.Debug.WriteLine($"Found {roleClaims.Length} claims of type '{roleClaimType}'");
                                    foreach (Claim roleClaim in roleClaims)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  - Role claim: Type={roleClaim.Type}, Value={roleClaim.Value}, Issuer={roleClaim.Issuer}");
                                        if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleClaim.Value))
                                        {
                                            identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleClaim.Value));
                                            System.Diagnostics.Debug.WriteLine($"Added 'role' claim from UserInfo: {roleClaim.Value}");
                                            rolesAdded++;
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Role claim '{roleClaim.Value}' already exists, skipping");
                                        }
                                    }
                                }
                                System.Diagnostics.Debug.WriteLine($"=== Total roles added from UserInfo: {rolesAdded} ===");
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
            });

        return services;
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
                System.Diagnostics.Debug.WriteLine("Invalid JWT format: access_token does not have the expected structure");
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

            System.Diagnostics.Debug.WriteLine($"Access token payload: {payloadJson}");

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
                            if (!identity.HasClaim(AuthorizationConstants.RoleClaimType, roleValue))
                            {
                                identity.AddClaim(new Claim(AuthorizationConstants.RoleClaimType, roleValue));
                                System.Diagnostics.Debug.WriteLine($"Added 'role' claim from access_token: {roleValue}");
                                rolesAdded++;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Role claim '{roleValue}' already exists, skipping");
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
                            System.Diagnostics.Debug.WriteLine($"Added 'role' claim from access_token: {roleValue}");
                            rolesAdded++;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Role claim '{roleValue}' already exists, skipping");
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No 'role' claim found in access_token payload");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error extracting roles from access_token: {ex.Message}");
        }

        return rolesAdded;
    }
}

