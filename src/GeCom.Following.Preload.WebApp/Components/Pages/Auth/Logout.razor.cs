using Microsoft.AspNetCore.Components;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Auth;

/// <summary>
/// Component for handling user logout.
/// </summary>
public partial class Logout : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Initializes the component and performs logout by redirecting to the logout API endpoint.
    /// </summary>
    protected override void OnInitialized()
    {
        // Redirect to the logout API endpoint which will handle the logout properly
        NavigationManager.NavigateTo("/api/auth/logout", forceLoad: true);
    }
}

