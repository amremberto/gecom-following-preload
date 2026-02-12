using GeCom.Following.Preload.Contracts.Preload.Documents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Modals;

public partial class ConfirmDeleteDocumentModal : ComponentBase
{
    private DocumentResponse? _selectedDocument;

    [Parameter]
    public EventCallback<DocumentResponse> OnConfirmDelete { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// Shows the modal with the specified document.
    /// </summary>
    /// <param name="document">The document to confirm deletion for.</param>
    public async Task ShowAsync(DocumentResponse document)
    {
        _selectedDocument = document;
        StateHasChanged();
        await JsRuntime.InvokeVoidAsync("eval", "new bootstrap.Modal(document.getElementById('confirmDeleteDocumentModal')).show()");
    }

    /// <summary>
    /// Handles the confirm button click: closes the modal and notifies the parent.
    /// </summary>
    private async Task HandleConfirm()
    {
        if (_selectedDocument is null)
        {
            return;
        }

        await JsRuntime.InvokeVoidAsync("eval", "bootstrap.Modal.getInstance(document.getElementById('confirmDeleteDocumentModal'))?.hide()");
        await OnConfirmDelete.InvokeAsync(_selectedDocument);
        _selectedDocument = null;
    }

    /// <summary>
    /// Closes the modal and resets state.
    /// </summary>
    private async Task CloseModal()
    {
        await OnCancel.InvokeAsync();
        _selectedDocument = null;
    }
}
