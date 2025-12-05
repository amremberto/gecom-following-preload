using System.Globalization;
using System.Security.Claims;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.WebApp.Components.Modals;
using GeCom.Following.Preload.WebApp.Configurations.Settings;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Documents;

public partial class Documents : IAsyncDisposable
{
    private bool _isLoading = true;
    private bool _isDataTableLoading;
    private bool _isModalLoading;
    private string _toastMessage = string.Empty;
    private IEnumerable<DocumentResponse> _documents = [];
    private IEnumerable<DocumentResponse> _pendingsDocuments = [];
    private bool _hasSupportedRole;
    private bool _isProvider;
    private string? _providerCuit;
    private bool _hasReadOnlyRole;
    private DocumentResponse? _selectedDocument;
    private string _selectedPdfFileName = string.Empty;

    private IJSObjectReference? _documentsModule;
    private DateOnly _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
    private DateOnly _dateTo = DateOnly.FromDateTime(DateTime.Today);

    public ProviderResponse? SelectedProvider { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private IProviderService ProviderService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IOptions<PreloadApiSettings> ApiSettings { get; set; } = default!;

    /// <summary>
    /// This method is called when the component is initialized.
    /// </summary>
    /// <returns></returns>
    protected override Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;

            StateHasChanged();

            // Set the initial date range
            _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
            _dateTo = DateOnly.FromDateTime(DateTime.Today);

            // Note: CheckIfProviderAndGetCuitAsync is called in OnAfterRenderAsync
            // because it uses JavaScript interop which is not available during pre-rendering
        }
        catch (Exception ex)
        {
            // Log error without JavaScript interop (will be logged in OnAfterRenderAsync if needed)
            System.Diagnostics.Debug.WriteLine($"Error al inicializar la página: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }

        return Task.CompletedTask;
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
                // Ensure _isLoading is false after initial render
                _isLoading = false;

                // Check if user is a provider and get their CUIT
                // This must be done here, not in OnInitializedAsync, because it uses JavaScript interop
                // which is not available during pre-rendering
                await CheckIfProviderAndGetCuitAsync();

                // Check if user has a supported role (must be done here, not in OnInitializedAsync
                // because JavaScript interop is not available during pre-rendering)
                _hasSupportedRole = await HasSupportedRoleAsync();

                // Check if user has ReadOnly role to hide certain UI elements
                _hasReadOnlyRole = await HasReadOnlyRoleAsync();
                StateHasChanged();

                _documentsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/components/table-datatable.min.js");
                await InvokeAsync(StateHasChanged); // Fuerza renderizado

                // If user has a supported role (Provider, Societies, Administrator, or ReadOnly), automatically load documents
                // The new endpoint will handle filtering automatically based on role
                if (_isProvider || _hasSupportedRole)
                {
                    try
                    {
                        _isDataTableLoading = true;
                        StateHasChanged();

                        await JsRuntime.InvokeVoidAsync("destroyDataTable", "documents-datatable");
                        await GetDocuments();
                        await JsRuntime.InvokeVoidAsync("loadDataTable", "documents-datatable");

                        await JsRuntime.InvokeVoidAsync("destroyDataTable", "pending-documents-datatable");
                        await GetPendingsDocuments();
                        await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");
                    }
                    finally
                    {
                        _isDataTableLoading = false;
                        StateHasChanged();
                    }
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "documents-datatable");
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");
                }

                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");
                await InitializeDatePickers();
            }
        }
        catch (Exception ex)
        {
            // Ensure loading states are reset on error
            _isLoading = false;
            _isDataTableLoading = false;
            await JsRuntime.InvokeVoidAsync("console.error", "Error en Documents.OnAfterRenderAsync:", ex.Message);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Get documents based on the date range and user role.
    /// Uses the new unified endpoint that handles filtering automatically based on role:
    /// - Providers: Filters by provider CUIT from claim
    /// - Societies: Filters by all societies assigned to the user
    /// - Administrator/ReadOnly: Returns all documents
    /// Falls back to the old endpoint if a provider is manually selected (for backward compatibility).
    /// </summary>
    /// <returns></returns>
    private async Task GetDocuments()
    {
        try
        {
            StateHasChanged();

            // Check user roles to determine which endpoint to use
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            bool hasProviderRole = false;
            bool hasSocietiesRole = false;
            bool hasAdministratorRole = false;
            bool hasReadOnlyRole = false;

            if (user is not null)
            {
                hasProviderRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                    user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                    user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadProviders);

                hasSocietiesRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadSocieties) ||
                    user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadSocieties) ||
                    user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadSocieties);

                hasAdministratorRole = user.IsInRole(AuthorizationConstants.Roles.FollowingAdministrator) ||
                    user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingAdministrator) ||
                    user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingAdministrator);

                hasReadOnlyRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadReadOnly) ||
                    user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadReadOnly) ||
                    user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadReadOnly);
            }

            IEnumerable<DocumentResponse>? response;

            // Use the new unified endpoint if user has one of the supported roles
            if (hasProviderRole || hasSocietiesRole || hasAdministratorRole || hasReadOnlyRole)
            {
                // The backend will automatically filter based on the user's role
                response = await DocumentService.GetByDatesAsync(_dateFrom, _dateTo, cancellationToken: default);
            }
            else if (SelectedProvider is not null)
            {
                // Fallback: If a provider is manually selected, use the old endpoint
                // This maintains backward compatibility for other scenarios
                response = await DocumentService.GetByDatesAndProviderAsync(
                    _dateFrom,
                    _dateTo,
                    SelectedProvider.Cuit,
                    cancellationToken: default);
            }
            else
            {
                await ShowToast("El proveedor es requerido o no tiene los permisos necesarios.");
                _documents = [];
                return;
            }

            _documents = response ?? [];
        }
        catch (ApiRequestException httpEx)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos:", httpEx.Message);
            await ShowToast(httpEx.Message);
            _documents = [];
        }
        catch (UnauthorizedAccessException)
        {
            await ShowToast("No tiene autorización para realizar esta operación. Por favor, inicie sesión nuevamente.");
            _documents = [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos:", ex.Message);
            await ShowToast("Ocurrió un error al buscar los documentos. Por favor, intente nuevamente.");
            _documents = [];
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

            // Check if the provider is selected (only if user is not a provider and doesn't have a supported role)
            // Users with roles: Provider, Societies, Administrator, or ReadOnly don't need to select a provider
            if (!_isProvider && !_hasSupportedRole && SelectedProvider == null)
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
        await OpenDocumentEdit(document.DocId);
    }

    /// <summary>
    /// Opens the document edit modal.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <returns></returns>
    private async Task OpenDocumentEdit(int docId)
    {
        _isModalLoading = true;
        _selectedDocument = null;
        _selectedPdfFileName = string.Empty;
        StateHasChanged();

        await GetDocumentWithDetails(docId);

        _isModalLoading = false;
        StateHasChanged();

        if (_selectedDocument is not null)
        {
            // Show edit modal
            await JsRuntime.InvokeVoidAsync("eval", "new bootstrap.Modal(document.getElementById('edit-document-modal')).show()");
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al abrir el modal de edición del documento: respuesta nula");
            await ShowToast("Error al cargar el documento para editar.");
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
    /// Checks if the current user is a provider and gets their CUIT from the claim.
    /// </summary>
    /// <returns></returns>
    private async Task CheckIfProviderAndGetCuitAsync()
    {
        try
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            if (user is null)
            {
                _isProvider = false;
                _providerCuit = null;
                return;
            }

            // Check if user has the provider role
            bool hasProviderRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadProviders) ||
                user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadProviders);

            if (hasProviderRole)
            {
                _isProvider = true;

                // Get CUIT from claim
                Claim? cuitClaim = user.FindFirst(AuthorizationConstants.SocietyCuitClaimType);
                if (cuitClaim is not null && !string.IsNullOrWhiteSpace(cuitClaim.Value))
                {
                    _providerCuit = cuitClaim.Value;
                    await JsRuntime.InvokeVoidAsync("console.log", $"[CheckIfProviderAndGetCuitAsync] User is a provider with CUIT: {_providerCuit}");
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync("console.warn", "[CheckIfProviderAndGetCuitAsync] User has provider role but no CUIT claim found");
                    _providerCuit = null;
                }
            }
            else
            {
                _isProvider = false;
                _providerCuit = null;
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"[CheckIfProviderAndGetCuitAsync] Error: {ex.Message}");
            _isProvider = false;
            _providerCuit = null;
        }
    }

    /// <summary>
    /// Checks if the current user has a supported role for document access.
    /// Supported roles: Administrator, Societies, Providers, or ReadOnly.
    /// Roles should be mapped during authentication, so we only need to check claims.
    /// </summary>
    /// <returns>True if the user has one of the supported roles, false otherwise.</returns>
    private async Task<bool> HasSupportedRoleAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;

        if (user is null)
        {
            await JsRuntime.InvokeVoidAsync("console.log", "[HasSupportedRoleAsync] User is null");
            return false;
        }

        // Check for supported roles that can access documents
        string[] targetRoles =
        [
            AuthorizationConstants.Roles.FollowingAdministrator,
            AuthorizationConstants.Roles.FollowingPreloadSocieties,
            AuthorizationConstants.Roles.FollowingPreloadProviders,
            AuthorizationConstants.Roles.FollowingPreloadReadOnly
        ];

        // Log all claims for debugging
        await JsRuntime.InvokeVoidAsync("console.log", "[HasSupportedRoleAsync] === Checking roles ===");
        await JsRuntime.InvokeVoidAsync("console.log", $"[HasSupportedRoleAsync] Looking for roles: {string.Join(", ", targetRoles)}");

        // Log all role claims
        var allRoleClaims = user.Claims.Where(c =>
            c.Type == ClaimTypes.Role ||
            c.Type == AuthorizationConstants.RoleClaimType ||
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .ToList();

        await JsRuntime.InvokeVoidAsync("console.log", $"[HasSupportedRoleAsync] Found {allRoleClaims.Count} role claims:");
        foreach (Claim claim in allRoleClaims)
        {
            await JsRuntime.InvokeVoidAsync("console.log", $"[HasSupportedRoleAsync]   - Type: {claim.Type}, Value: {claim.Value}");
        }

        // Check each target role
        foreach (string targetRole in targetRoles)
        {
            bool isInRole = user.IsInRole(targetRole);
            bool hasClaimTypesRole = user.HasClaim(ClaimTypes.Role, targetRole);
            bool hasRoleClaimType = user.HasClaim(AuthorizationConstants.RoleClaimType, targetRole);

            await JsRuntime.InvokeVoidAsync("console.log",
                $"[HasSupportedRoleAsync] Role '{targetRole}': IsInRole={isInRole}, HasClaim(ClaimTypes.Role)={hasClaimTypesRole}, HasClaim(role)={hasRoleClaimType}");

            if (isInRole || hasClaimTypesRole || hasRoleClaimType)
            {
                await JsRuntime.InvokeVoidAsync("console.log", $"[HasSupportedRoleAsync] ✓ User HAS role: {targetRole}");
                return true;
            }
        }

        await JsRuntime.InvokeVoidAsync("console.log", "[HasSupportedRoleAsync] ✗ User does NOT have required roles");
        return false;
    }

    /// <summary>
    /// Checks if the current user has the ReadOnly role.
    /// </summary>
    /// <returns>True if the user has the ReadOnly role, false otherwise.</returns>
    private async Task<bool> HasReadOnlyRoleAsync()
    {
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;

        if (user is null)
        {
            return false;
        }

        bool hasReadOnlyRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadReadOnly) ||
            user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadReadOnly) ||
            user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadReadOnly);

        return hasReadOnlyRole;
    }

    /// <summary>
    /// Checks if the document can be deleted based on status and user role.
    /// </summary>
    /// <param name="document">The document to check.</param>
    /// <returns>True if the document can be deleted, false otherwise.</returns>
    private bool CanDeleteDocument(DocumentResponse document)
    {
        bool hasCorrectStatus = document.EstadoDescripcion?.Trim().ToUpperInvariant() == "PRECARGA PENDIENTE";
        bool canDelete = hasCorrectStatus && _hasSupportedRole;

        // Log for debugging (async logging in sync method - fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("console.log",
                    $"[CanDeleteDocument] DocId: {document.DocId}, Status: {document.EstadoDescripcion}, " +
                    $"hasCorrectStatus: {hasCorrectStatus}, _hasSupportedRole: {_hasSupportedRole}, canDelete: {canDelete}");
            }
            catch
            {
                // Ignore errors in logging
            }
        });

        return canDelete;
    }

    private PreloadDocumentModal? _preloadModal;

    /// <summary>
    /// Handles the document click event to open the document details modal.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <returns></returns>
    private async Task OpenDocumentDetails(int docId)
    {
        _isModalLoading = true;
        _selectedDocument = null;
        _selectedPdfFileName = string.Empty;
        StateHasChanged();

        await GetDocumentWithDetails(docId);

        _isModalLoading = false;
        StateHasChanged();

        if (_selectedDocument is not null)
        {
            await JsRuntime.InvokeVoidAsync("destroyDataTable", "document-oc-datatable");
            await JsRuntime.InvokeVoidAsync("loadDataTable", "document-oc-datatable");
            await JsRuntime.InvokeVoidAsync("destroyDataTable", "document-notes-datatable");
            await JsRuntime.InvokeVoidAsync("loadDataTable", "document-notes-datatable");
            StateHasChanged();
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al abrir el modal de detalles del documento: respuesta nula");
        }
    }

    /// <summary>
    /// Retrieves the document details based on the document ID.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <returns></returns>
    private async Task GetDocumentWithDetails(int docId)
    {
        try
        {
            StateHasChanged();

            DocumentResponse? response = await DocumentService.GetByIdAsync(docId, cancellationToken: default);

            if (response is null)
            {
                await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar el documento: respuesta nula");
                _selectedDocument = null;
                return;
            }

            _selectedDocument = response;
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos:", ex.Message);
            _selectedDocument = null;
        }
        finally
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles the create new document action.
    /// </summary>
    /// <returns></returns>
    private async Task CreateNewDocument()
    {
        try
        {
            if (_preloadModal is not null)
            {
                await _preloadModal.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al abrir modal de precarga:", ex.Message);
            await ShowToast("Error al intentar abrir el formulario de precarga.");
        }
    }

    /// <summary>
    /// Handles the document preloaded event from the modal.
    /// </summary>
    /// <param name="document">The preloaded document.</param>
    /// <returns></returns>
    private async Task OnDocumentPreloaded(DocumentResponse document)
    {
        try
        {
            await ShowToast($"Documento #{document.DocId} precargado exitosamente.");

            // Navigate to document detail page
            NavigationManager.NavigateTo($"/documents/{document.DocId}");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al navegar al documento:", ex.Message);
            await ShowToast("Error al navegar al documento creado.");
        }
    }

    /// <summary>
    /// Handles the PDF file selection in the modals.
    /// </summary>
    /// <param name="e">The input file change event arguments.</param>
    /// <returns></returns>
    private void HandlePdfFileSelected(InputFileChangeEventArgs e)
    {
        IBrowserFile file = e.File;

        // Validate file type
        if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !file.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            _selectedPdfFileName = string.Empty;
            return;
        }

        // Validate file size (6 MB max)
        const long maxFileSize = 6 * 1024 * 1024; // 6 MB
        if (file.Size > maxFileSize)
        {
            _selectedPdfFileName = string.Empty;
            return;
        }

        _selectedPdfFileName = file.Name;
        StateHasChanged();
    }

    /// <summary>
    /// Checks if the selected document has any attachments.
    /// </summary>
    /// <returns>True if the document has attachments, false otherwise.</returns>
    private bool HasAttachment()
    {
        return _selectedDocument is not null &&
               _selectedDocument.Attachments is not null &&
               _selectedDocument.Attachments.Any(a => a.FechaBorrado is null);
    }

    /// <summary>
    /// Gets the ID of the first active attachment.
    /// </summary>
    /// <returns>The attachment ID, or 0 if no attachment is found.</returns>
    private int GetFirstAttachmentId()
    {
        if (_selectedDocument?.Attachments is null)
        {
            return 0;
        }

        AttachmentResponse? activeAttachment = _selectedDocument.Attachments
            .FirstOrDefault(a => a.FechaBorrado is null);

        return activeAttachment?.AdjuntoId ?? 0;
    }

    /// <summary>
    /// Handles PDF viewer errors.
    /// </summary>
    /// <returns></returns>
    private async Task HandlePdfError()
    {
        await ShowToast("Error al cargar el PDF. Por favor, intente nuevamente.");
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
