using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeCom.Following.Preload.WebApp.Controllers;

/// <summary>
/// Controller for handling account operations.
/// </summary>
[Route("[controller]/[action]")]
public sealed class AccountController(ILogger<AccountController> logger) : Controller
{
    /// <summary>
    /// Handles user logout by signing out via the OIDC scheme.
    /// Cookie cleanup is handled in OpenIdConnect events.
    /// </summary>
    /// <returns>Redirect to IdentityServer logout or home page.</returns>
    [HttpGet]
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        // If user is not authenticated, just redirect to home 
        if (User?.Identity?.IsAuthenticated is not true)
        {
            return LocalRedirect("~/");
        }

        logger.LogInformation("User {UserName} is logging out.", User.Identity!.Name);

        // If user is authenticated, sign out from both schemes (local and IdentityServer)
        // Create authentication properties with the id_token for proper logout
        AuthenticationProperties authProperties = new()
        {
            RedirectUri = Url.Content("~/")
        };

        return SignOut(authProperties, OpenIdConnectDefaults.AuthenticationScheme);
    }
}
