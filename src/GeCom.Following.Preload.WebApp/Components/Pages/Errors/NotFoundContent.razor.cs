using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Errors;

/// <summary>
/// Content component for 404 Not Found page, used within Router NotFound section.
/// </summary>
public partial class NotFoundContent
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





