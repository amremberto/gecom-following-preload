using System.Text.Json.Nodes;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components;

/// <summary>
/// Component for viewing PDF documents using PDF.js.
/// </summary>
public partial class PdfViewer : IAsyncDisposable
{
    [Parameter] public int AdjuntoId { get; set; }
    [Parameter] public EventCallback OnError { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IDocumentService DocumentService { get; set; } = default!;

    private bool _isLoading = true;
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private readonly string _canvasId = $"pdf-canvas-{Guid.NewGuid():N}";
    private int _currentPage = 1;
    private int _totalPages;
    private int _zoomLevel = 100;
    private IJSObjectReference? _pdfViewerModule;
    private bool _isLoadingPdf;
    private int _lastLoadedAdjuntoId;

    /// <summary>
    /// Called when parameters are set or updated.
    /// </summary>
    protected override Task OnParametersSetAsync()
    {
        // Reset state when AdjuntoId changes
        if (AdjuntoId > 0 && AdjuntoId != _lastLoadedAdjuntoId)
        {
            _isLoading = true;
            _hasError = false;
            _errorMessage = string.Empty;
            _totalPages = 0;
            _currentPage = 1;
            _isLoadingPdf = false;
            _lastLoadedAdjuntoId = 0; // Reset to allow reload
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after the component has rendered.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await EnsureModuleLoadedAsync();
        }

        // Load PDF after DOM is ready and AdjuntoId is valid
        // Only load if not already loading, not already loaded, and AdjuntoId changed
        if (AdjuntoId > 0 && 
            !_isLoadingPdf && 
            _lastLoadedAdjuntoId != AdjuntoId &&
            _pdfViewerModule is not null)
        {
            // Small delay to ensure DOM is fully ready, especially in modals
            await Task.Delay(200);
            await LoadPdfAsync();
        }
    }

    /// <summary>
    /// Ensures the PDF viewer JavaScript module is loaded.
    /// </summary>
    private async Task EnsureModuleLoadedAsync()
    {
        if (_pdfViewerModule is not null)
        {
            return;
        }

        try
        {
            _pdfViewerModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/pdf-viewer.js");
        }
        catch (JSException jsEx)
        {
            _hasError = true;
            _errorMessage = $"Error al cargar el módulo JavaScript del visor: {jsEx.Message}";
            await OnError.InvokeAsync();
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = $"Error al inicializar el visor de PDF: {ex.Message}";
            await OnError.InvokeAsync();
        }
    }

    /// <summary>
    /// Loads the PDF document from the attachment.
    /// </summary>
    private async Task LoadPdfAsync()
    {
        // Prevent multiple simultaneous loads
        if (_isLoadingPdf)
        {
            return;
        }

        try
        {
            _isLoadingPdf = true;
            _isLoading = true;
            _hasError = false;
            _errorMessage = string.Empty;
            _lastLoadedAdjuntoId = AdjuntoId; // Mark as loading for this AdjuntoId
            StateHasChanged();

            // Ensure module is loaded
            await EnsureModuleLoadedAsync();

            if (_pdfViewerModule is null)
            {
                _hasError = true;
                _errorMessage = "Error al inicializar el visor de PDF.";
                await OnError.InvokeAsync();
                return;
            }

            // Use proxy endpoint URL (same origin, no token exposure)
            // The proxy endpoint handles authentication server-side
            string proxyUrl = $"/api/AttachmentsProxy/{AdjuntoId}/download";
            
            // Load PDF document directly from URL (more efficient than Base64)
            // Wrap in try-catch to get more detailed error information
            JsonObject? result;
            try
            {
                // Call the exported function directly from the module
                result = await _pdfViewerModule.InvokeAsync<JsonObject>(
                    "loadPdfFromUrl", proxyUrl);
            }
            catch (JSException jsEx)
            {
                // Log the full error message from JavaScript
                _hasError = true;
                _errorMessage = $"Error al cargar el PDF desde la URL: {jsEx.Message}";
                if (jsEx.InnerException is not null)
                {
                    _errorMessage += $" Detalles: {jsEx.InnerException.Message}";
                }
                await OnError.InvokeAsync();
                return;
            }

            if (result is null)
            {
                _hasError = true;
                _errorMessage = "Error al procesar el PDF. El documento podría estar corrupto.";
                await OnError.InvokeAsync();
                return;
            }

            _totalPages = result["totalPages"]?.GetValue<int>() ?? 0;
            
            if (_totalPages == 0)
            {
                _hasError = true;
                _errorMessage = "El PDF no contiene páginas válidas.";
                await OnError.InvokeAsync();
                return;
            }

            _currentPage = 1;
            
            // Change state to show canvas (stop loading, show viewer)
            _isLoading = false;
            StateHasChanged();
            
            // Wait for DOM to update and canvas to be available
            // Use a more robust method to wait for canvas
            int retries = 0;
            const int maxRetries = 10;
            bool canvasExists = false;
            
            while (!canvasExists && retries < maxRetries)
            {
                await Task.Delay(100);
                canvasExists = await JSRuntime.InvokeAsync<bool>(
                    "eval", $"document.getElementById('{_canvasId}') !== null");
                retries++;
            }
            
            if (!canvasExists)
            {
                _hasError = true;
                _errorMessage = "El elemento canvas no está disponible en el DOM después de cargar el PDF.";
                await OnError.InvokeAsync();
                return;
            }
            
            // Render first page
            await RenderPageAsync(1);
        }
        catch (TaskCanceledException)
        {
            _hasError = true;
            _errorMessage = "La descarga del PDF tardó demasiado tiempo. Por favor, intente nuevamente.";
            await OnError.InvokeAsync();
        }
        catch (OperationCanceledException)
        {
            _hasError = true;
            _errorMessage = "La descarga del PDF fue cancelada. Por favor, intente nuevamente.";
            await OnError.InvokeAsync();
        }
        catch (Services.ApiRequestException apiEx)
        {
            _hasError = true;
            _errorMessage = apiEx.Message;
            await OnError.InvokeAsync();
        }
        catch (JSException jsEx)
        {
            _hasError = true;
            // Use the error message from JavaScript (which may have been improved)
            _errorMessage = jsEx.Message;
            if (string.IsNullOrWhiteSpace(_errorMessage))
            {
                _errorMessage = "Error de JavaScript al cargar el PDF. Por favor, intente nuevamente.";
            }
            await OnError.InvokeAsync();
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = $"Error al cargar el PDF: {ex.Message}";
            await OnError.InvokeAsync();
        }
        finally
        {
            // Only set loading to false if we haven't already done so (success case)
            // In case of error, _isLoading might already be false
            if (_isLoading)
            {
                _isLoading = false;
            }
            _isLoadingPdf = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Renders a specific page of the PDF.
    /// </summary>
    /// <param name="pageNumber">The page number to render (1-based).</param>
    private async Task RenderPageAsync(int pageNumber)
    {
        if (_pdfViewerModule is null)
        {
            return;
        }

        try
        {
            // Ensure canvas exists in DOM before rendering
            bool canvasExists = await JSRuntime.InvokeAsync<bool>(
                "eval", $"document.getElementById('{_canvasId}') !== null");
            
            if (!canvasExists)
            {
                // Wait a bit more and try again with retries
                int retries = 0;
                const int maxRetries = 5;
                while (!canvasExists && retries < maxRetries)
                {
                    await Task.Delay(100);
                    canvasExists = await JSRuntime.InvokeAsync<bool>(
                        "eval", $"document.getElementById('{_canvasId}') !== null");
                    retries++;
                }
                
                if (!canvasExists)
                {
                    _hasError = true;
                    _errorMessage = "El elemento canvas no está disponible en el DOM.";
                    StateHasChanged();
                    return;
                }
            }
            
            double scale = _zoomLevel / 100.0;
            
            // Call the exported function directly from the module
            await _pdfViewerModule.InvokeVoidAsync(
                "renderPage", _canvasId, pageNumber, scale);
            
            StateHasChanged();
        }
        catch (JSException jsEx)
        {
            _hasError = true;
            _errorMessage = $"Error de JavaScript al renderizar la página: {jsEx.Message}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = $"Error al renderizar la página: {ex.Message}";
            StateHasChanged();
        }
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    private async Task PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await RenderPageAsync(_currentPage);
        }
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    private async Task NextPage()
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            await RenderPageAsync(_currentPage);
        }
    }

    /// <summary>
    /// Increases the zoom level.
    /// </summary>
    private async Task ZoomIn()
    {
        if (_zoomLevel < 200)
        {
            _zoomLevel += 25;
            await RenderPageAsync(_currentPage);
        }
    }

    /// <summary>
    /// Decreases the zoom level.
    /// </summary>
    private async Task ZoomOut()
    {
        if (_zoomLevel > 50)
        {
            _zoomLevel -= 25;
            await RenderPageAsync(_currentPage);
        }
    }

    /// <summary>
    /// Resets the zoom level to 100%.
    /// </summary>
    private async Task ResetZoom()
    {
        _zoomLevel = 100;
        await RenderPageAsync(_currentPage);
    }

    /// <summary>
    /// Disposes the component and cleans up resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_pdfViewerModule is not null)
        {
            try
            {
                // Call the exported function directly from the module
                await _pdfViewerModule.InvokeVoidAsync("dispose");
                await _pdfViewerModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }

        GC.SuppressFinalize(this);
    }
}

