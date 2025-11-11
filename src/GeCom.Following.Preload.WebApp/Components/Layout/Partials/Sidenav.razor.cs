using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Layout.Partials;

public partial class Sidenav
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private async Task ToggleSidenav()
    {
        await JsRuntime.InvokeVoidAsync("toggleSidenav");
    }

    private async Task HandleRouteChange(string route)
    {
        string currentUri = NavigationManager.Uri;
        string newUri = NavigationManager.BaseUri + route.TrimStart('/');

        if (currentUri.Equals(newUri, StringComparison.OrdinalIgnoreCase))
        {
            await JsRuntime.InvokeVoidAsync("loadThemeConfig");
            await JsRuntime.InvokeVoidAsync("loadApps");
        }
        else
        {
            NavigationManager.NavigateTo(route);
        }
    }
}


