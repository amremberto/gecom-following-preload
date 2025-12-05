using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Documents;

/// <summary>
/// Component for displaying document details.
/// </summary>
public partial class DocumentDetail
{
    [Parameter] public int DocId { get; set; }

    private bool _isLoading = true;
    private DocumentResponse? _document;

    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Initializes the component and loads the document.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadDocument();
    }

    /// <summary>
    /// Loads the document by ID from the service.
    /// </summary>
    private async Task LoadDocument()
    {
        try
        {
            _isLoading = true;
            _document = await DocumentService.GetByIdAsync(DocId);
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error loading document: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Navigates back to the documents list page.
    /// </summary>
    private void NavigateToDocuments()
    {
        NavigationManager.NavigateTo("/documents");
    }

    /// <summary>
    /// Handles the edit document action.
    /// </summary>
    private void EditDocument()
    {
        // La edición se realizará en esta misma página en el futuro
        // Por ahora, la página muestra todos los detalles del documento
    }

    /// <summary>
    /// Gets the CSS class for the document status badge based on the status value.
    /// </summary>
    /// <param name="estado">The document status.</param>
    /// <returns>The CSS class for the badge.</returns>
    private static string GetEstadoBadgeClass(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return "bg-warning text-dark";
        }

        return estado.Trim().ToUpperInvariant() switch
        {
            "PRECARGADO" => "bg-info text-dark",
            "RECHAZO PRECARGA" => "bg-danger text-white",
            "PAGADO" => "bg-success text-white",
            _ => "bg-warning text-dark"
        };
    }
}

