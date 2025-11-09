using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeCom.Following.Preload.WebApp.Controllers;

/// <summary>
/// Controller for handling authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Handles user logout by signing out from both cookie and OIDC schemes.
    /// </summary>
    /// <returns>Redirect to IdentityServer logout or home page.</returns>
    [HttpPost("logout")]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        // If user is authenticated, sign out from both schemes
        if (User.Identity?.IsAuthenticated == true)
        {
            // Get the id_token before signing out, as it may not be available after
            string? idToken = await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "id_token");
            
            // Create authentication properties with the id_token for proper logout
            AuthenticationProperties authProperties = new()
            {
                RedirectUri = "/"
            };
            
            // Store the id_token in properties so it's available in OnRedirectToIdentityProviderForSignOut
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                authProperties.Items["id_token"] = idToken;
            }

            // Sign out from OIDC scheme first - this will trigger the OnRedirectToIdentityProviderForSignOut event
            // which will redirect to IdentityServer for logout
            // The OIDC middleware will handle the redirect automatically
            await HttpContext.SignOutAsync(
                OpenIdConnectDefaults.AuthenticationScheme,
                authProperties);
            
            // After OIDC sign out, the middleware should have redirected to IdentityServer
            // If we reach here, it means the redirect didn't happen, so sign out from cookie and redirect to home
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        // If we reach here, redirect to home
        return Redirect("/");
    }
}

