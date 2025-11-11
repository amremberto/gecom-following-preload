using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Layout.Partials;

public partial class Topbar
{
    [Parameter] public required string Title { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private async Task ToggleSidenav()
    {
        await JsRuntime.InvokeVoidAsync("toggleSidenav");
    }

    private void OnLogoutClicked()
    {
        // Navegación forzada a través de la pipeline completa (no solo router Blazor)
        NavigationManager.NavigateTo("Account/Logout", forceLoad: true);
    }
}


