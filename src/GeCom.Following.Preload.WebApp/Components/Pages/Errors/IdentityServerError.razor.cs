using Microsoft.AspNetCore.Components;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Errors;

/// <summary>
/// Component for displaying Identity Server connection error page.
/// </summary>
public partial class IdentityServerError
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Retries the connection by navigating to the home page with force reload.
    /// </summary>
    private void RetryConnection()
    {
        // Forzar recarga de la página para reintentar la conexión
        NavigationManager.NavigateTo("/", forceLoad: true);
    }
}







