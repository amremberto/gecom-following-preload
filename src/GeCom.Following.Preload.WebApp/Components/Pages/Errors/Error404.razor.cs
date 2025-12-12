using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Errors;

/// <summary>
/// Component for displaying 404 Not Found error page.
/// </summary>
public partial class Error404
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Navigates back to the previous page in browser history.
    /// </summary>
    private async Task GoBack()
    {
        await JSRuntime.InvokeVoidAsync("history.back");
    }
}







