using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages;

public partial class Dashboard : IAsyncDisposable
{
    private IJSObjectReference? _dashboardModule;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dashboardModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/pages/dashboard.js");
            await JsRuntime.InvokeVoidAsync("loadDashboard");
            await JsRuntime.InvokeVoidAsync("loadThemeConfig");
            await JsRuntime.InvokeVoidAsync("loadApps");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_dashboardModule is not null)
        {
            await _dashboardModule.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}
