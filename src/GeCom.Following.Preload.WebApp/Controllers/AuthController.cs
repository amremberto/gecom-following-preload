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
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Sign out from cookie scheme first
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Sign out from OIDC scheme - this will trigger the OnRedirectToIdentityProviderForSignOut event
        // which will redirect to IdentityServer for logout
        await HttpContext.SignOutAsync(
            OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = "/"
            });

        // If we reach here, redirect to home
        return Redirect("/");
    }
}

