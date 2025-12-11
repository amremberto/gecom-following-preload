using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Layout.Partials;

public partial class Sidenav
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private async Task ToggleSidenav()
    {
        await JsRuntime.InvokeVoidAsync("toggleSidenav");
    }
}
