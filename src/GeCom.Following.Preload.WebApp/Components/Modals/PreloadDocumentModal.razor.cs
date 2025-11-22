using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Modals;

public partial class PreloadDocumentModal : ComponentBase
{
    private bool _isUploading;
    private bool _hasFileSelected;
    private string _selectedFileName = string.Empty;
    private string _errorMessage = string.Empty;
    private IBrowserFile? _selectedFile;
    private readonly object _preloadModel = new();

    [Parameter] public EventCallback<DocumentResponse> OnDocumentPreloaded { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private void HandleFileSelected(InputFileChangeEventArgs e)
    {
        _errorMessage = string.Empty;
        _selectedFile = e.File;

        // Validate file type
        if (!string.Equals(_selectedFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !_selectedFile.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            _errorMessage = "Solo se permiten archivos PDF.";
            _hasFileSelected = false;
            _selectedFileName = string.Empty;
            return;
        }

        // Validate file size (6 MB max)
        const long maxFileSize = 6 * 1024 * 1024; // 6 MB
        if (_selectedFile.Size > maxFileSize)
        {
            _errorMessage = $"El archivo excede el tamaño máximo de 6 MB. Tamaño actual: {_selectedFile.Size / 1024.0 / 1024.0:F2} MB.";
            _hasFileSelected = false;
            _selectedFileName = string.Empty;
            return;
        }

        _hasFileSelected = true;
        _selectedFileName = _selectedFile.Name;
    }

    private async Task HandleFileUpload()
    {
        if (_selectedFile is null || !_hasFileSelected)
        {
            _errorMessage = "Por favor seleccione un archivo PDF.";
            return;
        }

        try
        {
            _isUploading = true;
            _errorMessage = string.Empty;
            StateHasChanged();

            DocumentResponse? document = await DocumentService.PreloadDocumentAsync(_selectedFile);

            if (document is null)
            {
                _errorMessage = "No se pudo crear el documento. Por favor intente nuevamente.";
                return;
            }

            // Close modal
            await JsRuntime.InvokeVoidAsync("eval", "bootstrap.Modal.getInstance(document.getElementById('preloadDocumentModal'))?.hide()");

            // Notify parent component
            await OnDocumentPreloaded.InvokeAsync(document);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error al subir el archivo: {ex.Message}";
        }
        finally
        {
            _isUploading = false;
            StateHasChanged();
        }
    }

    private async Task CloseModal()
    {
        await OnCancel.InvokeAsync();
        ResetForm();
    }

    private void ResetForm()
    {
        _selectedFile = null;
        _hasFileSelected = false;
        _selectedFileName = string.Empty;
        _errorMessage = string.Empty;
        _isUploading = false;
    }

    public async Task ShowAsync()
    {
        ResetForm();
        await JsRuntime.InvokeVoidAsync("eval", "new bootstrap.Modal(document.getElementById('preloadDocumentModal')).show()");
    }
}

