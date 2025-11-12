using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages;

public partial class Dashboard : IAsyncDisposable
{
    private bool _isLoading = true;

    private int _totalDocuments;
    private int _totalPurchaseOrders;
    private int _totalPendingDocuments;

    private IJSObjectReference? _dashboardModule;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// This method is called after the component has been rendered.
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                _dashboardModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/pages/dashboard.min.js");
                await JsRuntime.InvokeVoidAsync("loadDashboard");
                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");
            }

        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error en Dashboard.OnAfterRenderAsync:", ex.Message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_dashboardModule is not null)
        {
            try
            {
                await _dashboardModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // El circuito ya está desconectado, no podemos hacer llamadas de JavaScript interop
                // Esto es normal cuando el componente se está eliminando
            }
        }
        GC.SuppressFinalize(this);
    }
}
