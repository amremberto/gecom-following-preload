using Microsoft.AspNetCore.Components;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Errors;

/// <summary>
/// Component for displaying 500 Internal Server Error page.
/// </summary>
public partial class Error500
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Retries the current page by forcing a reload.
    /// </summary>
    private void Retry()
    {
        // Forzar recarga de la página actual
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }
}





