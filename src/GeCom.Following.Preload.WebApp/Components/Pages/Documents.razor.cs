using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages;

public partial class Documents : IAsyncDisposable
{
    private bool _isLoading = true;
    private IJSObjectReference? _documentsModule;
    private DateOnly _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
    private DateOnly _dateTo = DateOnly.FromDateTime(DateTime.Today);

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// This method is called when the component is initialized.
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;

            StateHasChanged();

            // Set the initial date range
            _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
            _dateTo = DateOnly.FromDateTime(DateTime.Today);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar la página:", ex.Message);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                _documentsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/components/table-datatable.min.js");
                await InvokeAsync(StateHasChanged); // Fuerza renderizado
                await JsRuntime.InvokeVoidAsync("loadDataTable", "documents-datatable");
                await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");
                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");
                await InitializeDatePickers();
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error en Dashboard.OnAfterRenderAsync:", ex.Message);
        }
    }

    /// <summary>
    /// Initializes the date pickers using JavaScript interop.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeDatePickers()
    {
        await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation", "#dateFrom", new { defaultDate = _dateFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });

        await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation", "#dateTo", new { defaultDate = _dateTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
    }

    public async ValueTask DisposeAsync()
    {
        if (_documentsModule is not null)
        {
            try
            {
                await _documentsModule.DisposeAsync();
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
