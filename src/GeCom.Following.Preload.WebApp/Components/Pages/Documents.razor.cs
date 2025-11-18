using System.Globalization;
using System.Linq.Expressions;
using System.Security.Claims;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages;

public partial class Documents : IAsyncDisposable
{
    private bool _isLoading = true;
    private bool _isDataTableLoading;
    private string _toastMessage = string.Empty;
    private IEnumerable<DocumentResponse> _documents = [];
    private IEnumerable<DocumentResponse> _pendingsDocuments = [];
    private bool _hasAllSocietiesRole;

    private IJSObjectReference? _documentsModule;
    private DateOnly _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
    private DateOnly _dateTo = DateOnly.FromDateTime(DateTime.Today);

    public ProviderResponse? SelectedProvider { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private IProviderService ProviderService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

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

    /// <summary>
    /// This method is called after the component has rendered.
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                // Check if user has AllSocieties role (must be done here, not in OnInitializedAsync
                // because JavaScript interop is not available during pre-rendering)
                _hasAllSocietiesRole = await HasAllSocietiesRoleAsync();
                StateHasChanged();

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
    /// Get documents based on the date range and selected provider.
    /// </summary>
    /// <returns></returns>
    private async Task GetDocuments()
    {
        try
        {
            StateHasChanged();

            IEnumerable<DocumentResponse>? response =
                await DocumentService.GetByDatesAndProviderAsync(_dateFrom, _dateTo, SelectedProvider!.Cuit, cancellationToken: default);

            _documents = response ?? [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos:", ex.Message);
        }
        finally
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Get the pending documents based on the document list.
    /// </summary>
    /// <returns></returns>
    private async Task GetPendingsDocuments()
    {
        try
        {
            StateHasChanged();

            // Filtrar los documentos ya obtenidos por EstadoId 2 o 5
            _pendingsDocuments = _documents.Where(d => d.EstadoId == 2 || d.EstadoId == 5).ToList();
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al filtrar los documentos pendientes:", ex.Message);
        }
        finally
        {
            StateHasChanged();
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

    /// <summary>
    /// Handles the search button click event for searching documents.
    /// </summary>
    /// <returns></returns>
    private async Task SearchDocuments()
    {
        try
        {
            await ForceDateInputsBlurAsync();

            // Check if the date range is valid
            if (_dateFrom > _dateTo)
            {
                await ShowToast("La (fecha desde) no puede ser mayor a la (fecha hasta).");
                return;
            }

            // Check if the provider is selected
            if (SelectedProvider == null)
            {
                await ShowToast("El (proveedor) es requerido.");
                return;
            }

            _isDataTableLoading = true;
            StateHasChanged();

            await JsRuntime.InvokeVoidAsync("destroyDataTable", "documents-datatable");
            await GetDocuments();
            await JsRuntime.InvokeVoidAsync("loadDataTable", "documents-datatable");

            await JsRuntime.InvokeVoidAsync("destroyDataTable", "pending-documents-datatable");
            await GetPendingsDocuments();
            await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al filtrar los documentos pendientes:", ex.Message);
        }
        finally
        {
            _isDataTableLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Searches for providers based on the input text.
    /// </summary>
    /// <param name="searchText"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ProviderResponse>> SearchProvidersAsync(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return [];
        }

        try
        {
            IEnumerable<ProviderResponse>? response = await ProviderService.SearchAsync(searchText, default);

            return response ?? [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    /// <summary>
    /// Handles the selection of a provider from the combo box.
    /// </summary>
    /// <param name="selected"></param>
    /// <returns></returns>
    private Task OnProviderSelectedAsync(ProviderResponse selected)
    {
        SelectedProvider = selected;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the date change event for the date pickers.
    /// </summary>
    private readonly Expression<Func<ProviderResponse, string>> _textSelectorExprr =
        p => p.Cuit + " -- " + p.RazonSocial;

    /// <summary>
    /// Displays a toast message using JavaScript interop.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task ShowToast(string message)
    {
        _toastMessage = message;
        await JsRuntime.InvokeVoidAsync("showBlazorToast", "dateRangeToast");
        StateHasChanged();
    }

    /// <summary>
    /// Gets the CSS class for the document status badge based on the status value.
    /// </summary>
    /// <param name="estado"></param>
    /// <returns></returns>
    private static string GetEstadoBadgeClass(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return "badge bg-warning rounded-pill text-dark";
        }

        return estado.Trim().ToUpperInvariant() switch
        {
            "RECHAZO PRECARGA" => "badge bg-danger rounded-pill text-dark",
            "PENDIENTE PRECARGA" => "badge bg-danger rounded-pill text-dark",
            "PRECARGA PENDIENTE" => "badge bg-danger rounded-pill text-dark",
            "PAGADO" => "badge bg-success rounded-pill text-dark",
            "PAGO EMITIDO" => "badge bg-success rounded-pill text-dark",
            // Agrega más estados según tu dominio
            _ => "badge bg-warning rounded-pill text-dark"
        };
    }

    /// <summary>
    /// Forces the date inputs to lose focus.
    /// </summary>
    /// <returns></returns>
    private async Task ForceDateInputsBlurAsync()
    {
        await JsRuntime.InvokeVoidAsync("blurElementById", "dateFrom");
        await JsRuntime.InvokeVoidAsync("blurElementById", "dateTo");
    }

    /// <summary>
    /// Handles the edit document action.
    /// </summary>
    /// <param name="document">The document to edit.</param>
    /// <returns></returns>
    private async Task EditDocument(DocumentResponse document)
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("console.log", $"Editando documento con ID: {document.DocId}");
            // Implementar la lógica de edición (navegar a página de edición, abrir modal, etc.)
            await ShowToast($"Funcionalidad de edición para el documento {document.DocId} pendiente de implementar.");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al editar documento:", ex.Message);
            await ShowToast("Error al intentar editar el documento.");
        }
    }

    /// <summary>
    /// Handles the delete document action.
    /// </summary>
    /// <param name="document">The document to delete.</param>
    /// <returns></returns>
    private async Task DeleteDocument(DocumentResponse document)
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("console.log", $"Eliminando documento con ID: {document.DocId}");
            // Implementar la lógica de eliminación (confirmación, llamada al servicio, etc.)
            await ShowToast($"Funcionalidad de eliminación para el documento {document.DocId} pendiente de implementar.");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al eliminar documento:", ex.Message);
            await ShowToast("Error al intentar eliminar el documento.");
        }
    }

    /// <summary>
    /// Checks if the current user has the AllSocieties role.
    /// Roles should be mapped during authentication, so we only need to check claims.
    /// </summary>
    /// <returns>True if the user has the role, false otherwise.</returns>
    private async Task<bool> HasAllSocietiesRoleAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;

        if (user is null)
        {
            await JsRuntime.InvokeVoidAsync("console.log", "[HasAllSocietiesRoleAsync] User is null");
            return false;
        }

        // Check for both Administrator and PreloadAllSocieties roles
        string[] targetRoles =
        [
            AuthorizationConstants.Roles.FollowingAdministrator,
            AuthorizationConstants.Roles.FollowingPreloadAllSocieties
        ];

        // Log all claims for debugging
        await JsRuntime.InvokeVoidAsync("console.log", "[HasAllSocietiesRoleAsync] === Checking roles ===");
        await JsRuntime.InvokeVoidAsync("console.log", $"[HasAllSocietiesRoleAsync] Looking for roles: {string.Join(", ", targetRoles)}");

        // Log all role claims
        var allRoleClaims = user.Claims.Where(c =>
            c.Type == ClaimTypes.Role ||
            c.Type == AuthorizationConstants.RoleClaimType ||
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .ToList();

        await JsRuntime.InvokeVoidAsync("console.log", $"[HasAllSocietiesRoleAsync] Found {allRoleClaims.Count} role claims:");
        foreach (Claim claim in allRoleClaims)
        {
            await JsRuntime.InvokeVoidAsync("console.log", $"[HasAllSocietiesRoleAsync]   - Type: {claim.Type}, Value: {claim.Value}");
        }

        // Check each target role
        foreach (string targetRole in targetRoles)
        {
            bool isInRole = user.IsInRole(targetRole);
            bool hasClaimTypesRole = user.HasClaim(ClaimTypes.Role, targetRole);
            bool hasRoleClaimType = user.HasClaim(AuthorizationConstants.RoleClaimType, targetRole);

            await JsRuntime.InvokeVoidAsync("console.log",
                $"[HasAllSocietiesRoleAsync] Role '{targetRole}': IsInRole={isInRole}, HasClaim(ClaimTypes.Role)={hasClaimTypesRole}, HasClaim(role)={hasRoleClaimType}");

            if (isInRole || hasClaimTypesRole || hasRoleClaimType)
            {
                await JsRuntime.InvokeVoidAsync("console.log", $"[HasAllSocietiesRoleAsync] ✓ User HAS role: {targetRole}");
                return true;
            }
        }

        await JsRuntime.InvokeVoidAsync("console.log", "[HasAllSocietiesRoleAsync] ✗ User does NOT have required roles");
        return false;
    }

    /// <summary>
    /// Checks if the document can be deleted based on status and user role.
    /// </summary>
    /// <param name="document">The document to check.</param>
    /// <returns>True if the document can be deleted, false otherwise.</returns>
    private bool CanDeleteDocument(DocumentResponse document)
    {
        bool hasCorrectStatus = document.EstadoDescripcion?.Trim().ToUpperInvariant() == "PRECARGA PENDIENTE";
        bool canDelete = hasCorrectStatus && _hasAllSocietiesRole;

        // Log for debugging (async logging in sync method - fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("console.log",
                    $"[CanDeleteDocument] DocId: {document.DocId}, Status: {document.EstadoDescripcion}, " +
                    $"hasCorrectStatus: {hasCorrectStatus}, _hasAllSocietiesRole: {_hasAllSocietiesRole}, canDelete: {canDelete}");
            }
            catch
            {
                // Ignore errors in logging
            }
        });

        return canDelete;
    }

    /// <summary>
    /// Handles the create new document action.
    /// </summary>
    /// <returns></returns>
    private async Task CreateNewDocument()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("console.log", "Creando nuevo documento");
            // Implementar la lógica de creación (navegar a página de creación, abrir modal, etc.)
            await ShowToast("Funcionalidad de creación de nuevo documento pendiente de implementar.");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al crear nuevo documento:", ex.Message);
            await ShowToast("Error al intentar crear un nuevo documento.");
        }
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
