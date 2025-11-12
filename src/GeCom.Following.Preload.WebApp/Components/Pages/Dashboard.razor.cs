using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.WebApp.Services;
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
    [Inject] private IDashboardService DashboardService { get; set; } = default!;

    /// <summary>
    /// This method is called when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            // Log error (in production, use ILogger)
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            // Set default values on error
            _totalDocuments = 0;
            _totalPurchaseOrders = 0;
            _totalPendingDocuments = 0;
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Loads dashboard data from the API.
    /// </summary>
    private async Task LoadDashboardDataAsync()
    {
        DashboardResponse? dashboardResponse = await DashboardService.GetDashboardAsync();

        if (dashboardResponse is not null)
        {
            _totalDocuments = dashboardResponse.TotalDocuments;
            _totalPurchaseOrders = dashboardResponse.TotalPurchaseOrders;
            _totalPendingDocuments = dashboardResponse.TotalPendingsDocuments;
        }
    }

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
