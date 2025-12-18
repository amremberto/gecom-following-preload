using System.Globalization;
using System.Security.Claims;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.WebApp.Components.Modals;
using GeCom.Following.Preload.WebApp.Components.Shared;
using GeCom.Following.Preload.WebApp.Enums;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Documents;

public partial class Paid : IAsyncDisposable
{
    [Inject] private ILogger<Paid> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private IDocumentService DocumentService { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isDataTableLoading;
    private bool _isModalLoading;
    private bool _hasSupportedRole;
    private bool _isProvider;
    private string? _providerCuit;
    private bool _hasReadOnlyRole;

    private Toast? _toast;
    private DateOnly _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
    private DateOnly _dateTo = DateOnly.FromDateTime(DateTime.Today);
    public ProviderResponse? SelectedProvider { get; set; }
    private DocumentResponse? _selectedDocument;
    private string _selectedPdfFileName = string.Empty;
    private IEnumerable<DocumentResponse> _paidDocuments = [];

    private IJSObjectReference? _documentsModule;
    private PreloadDocumentModal? _preloadModal;

    /// <summary>
    /// This method is called when the component is initialized.
    /// </summary>
    /// <returns></returns>
    protected override Task OnInitializedAsync()
    {
        Logger.LogInformation("Entrando en Paid.OnInitializedAsync");

        // In Blazor Server with InteractiveServer, OnInitializedAsync is called twice:
        // 1. During server-side pre-rendering
        // 2. When the SignalR connection is established
        // To avoid duplicate API calls, we only initialize state here and load data in OnAfterRenderAsync
        _isLoading = true;
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                Logger.LogInformation("Entrando en Paid.OnAfterRenderAsync por primera vez");

                try
                {
                    // Ensure _isLoading is false after initial render
                    _isLoading = false;

                    // Initialize toast service with component reference
                    // Wait a bit to ensure Toast component is fully rendered
                    await Task.Delay(100);
                    if (_toast is not null)
                    {
                        ToastService.SetToast(_toast);
                    }

                    // Set the initial date range
                    _dateFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
                    _dateTo = DateOnly.FromDateTime(DateTime.Today);

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
                            await GetPaidDocuments();
                            await JsRuntime.InvokeVoidAsync("loadDataTable", "documents-datatable");
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
                    }
                }
                catch (Exception)
                {
                    // Set default values in case of error
                    _isProvider = false;
                    _providerCuit = null;
                    _hasSupportedRole = false;
                    _hasReadOnlyRole = false;
                }
                finally
                {
                    _isLoading = false;
                    StateHasChanged();
                }

                // Load JavaScript modules
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
    /// Fetches paid documents based on the selected date range and user roles.
    /// </summary>
    /// <returns></returns>
    private async Task GetPaidDocuments()
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

            // Use the new paid documents endpoint if user has one of the supported roles
            // This endpoint filters documents with state code "PagadoFin"
            if (hasProviderRole || hasSocietiesRole || hasAdministratorRole || hasReadOnlyRole)
            {
                // The backend will automatically filter paid documents based on the user's role
                response = await DocumentService.GetPaidDocumentsByDatesAsync(_dateFrom, _dateTo, cancellationToken: default);
            }
            else if (SelectedProvider is not null)
            {
                // Fallback: If a provider is manually selected, use the old endpoint
                // This maintains backward compatibility for other scenarios
                // Note: This endpoint doesn't filter by paid status, so we'll filter client-side if needed
                response = await DocumentService.GetByDatesAndProviderAsync(
                    _dateFrom,
                    _dateTo,
                    SelectedProvider.Cuit,
                    cancellationToken: default);
            }
            else
            {
                await ShowToast("El proveedor es requerido o no tiene los permisos necesarios.", ToastType.Warning);
                _paidDocuments = [];
                return;
            }

            _paidDocuments = response ?? [];
        }
        catch (ApiRequestException httpEx)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos:", httpEx.Message);
            await ShowToast(httpEx.Message);
            _paidDocuments = [];
        }
        catch (UnauthorizedAccessException)
        {
            await ShowToast("No tiene autorización para realizar esta operación. Por favor, inicie sesión nuevamente.");
            _paidDocuments = [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos:", ex.Message);
            await ShowToast("Ocurrió un error al buscar los documentos. Por favor, intente nuevamente.");
            _paidDocuments = [];
        }
        finally
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles the search button click event for searching documents.
    /// </summary>
    /// <returns></returns>
    private async Task SearchPaidDocuments()
    {
        try
        {
            await ForceDateInputsBlurAsync();

            // Check if the date range is valid
            if (_dateFrom > _dateTo)
            {
                await ShowToast("La (fecha desde) no puede ser mayor a la (fecha hasta).", ToastType.Warning);
                return;
            }

            // Check if the provider is selected (only if user is not a provider and doesn't have a supported role)
            // Users with roles: Provider, Societies, Administrator, or ReadOnly don't need to select a provider
            if (!_isProvider && !_hasSupportedRole && SelectedProvider == null)
            {
                await ShowToast("El (proveedor) es requerido.", ToastType.Warning);
                return;
            }

            _isDataTableLoading = true;
            StateHasChanged();

            await JsRuntime.InvokeVoidAsync("destroyDataTable", "documents-datatable");
            await GetPaidDocuments();
            await JsRuntime.InvokeVoidAsync("loadDataTable", "documents-datatable");
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
    /// Handles the document preloaded event from the modal.
    /// </summary>
    /// <param name="document">The preloaded document.</param>
    /// <returns></returns>
    private async Task OnDocumentPreloaded(DocumentResponse document)
    {
        try
        {
            // Navigate to Pending page with the document ID as query parameter
            // This will allow Pending page to automatically open the edit modal
            // The success message will be shown in Pending page after Toast is initialized
            NavigationManager.NavigateTo($"/documents/pending?editDocId={document.DocId}");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al navegar al documento:", ex.Message);
            // Only show error toast if we're still on this page
            try
            {
                await ShowToast("Error al navegar al documento creado.");
            }
            catch
            {
                // Toast might not be available if navigation already started
                await JsRuntime.InvokeVoidAsync("console.error", "Error al navegar al documento creado.");
            }
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
    /// Handles PDF viewer errors.
    /// </summary>
    /// <returns></returns>
    private async Task HandlePdfError()
    {
        try
        {
            // Verify Toast is initialized before showing error message
            if (_toast is not null)
            {
                await ShowToast("Error al cargar el PDF. El documento no se encontró o está corrupto.", ToastType.Error);
            }
            else
            {
                // Fallback: Log to console if Toast is not available
                await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar el PDF. El documento no se encontró o está corrupto.");
            }
        }
        catch (InvalidOperationException)
        {
            // Toast service not initialized - log to console as fallback
            try
            {
                await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar el PDF. El documento no se encontró o está corrupto.");
            }
            catch
            {
                // If even console logging fails, silently fail to prevent breaking the app
            }
        }
        catch (Exception ex)
        {
            // Log any other errors but don't break the app
            try
            {
                await JsRuntime.InvokeVoidAsync("console.error", $"Error al manejar el error del PDF: {ex.Message}");
            }
            catch
            {
                // Silently fail to prevent breaking the app
            }
        }
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
    /// Formats the document type display string (Codigo - Letra - Descripcion).
    /// </summary>
    /// <param name="document">The document response.</param>
    /// <returns>Formatted document type string.</returns>
    private static string FormatDocumentType(DocumentResponse? document)
    {
        if (document is null)
        {
            return "N/A";
        }

        if (string.IsNullOrWhiteSpace(document.TipoDocCodigo) &&
            string.IsNullOrWhiteSpace(document.TipoDocLetra) &&
            string.IsNullOrWhiteSpace(document.TipoDocDescripcion))
        {
            return "N/A";
        }

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(document.TipoDocCodigo))
        {
            parts.Add(document.TipoDocCodigo);
        }

        if (!string.IsNullOrWhiteSpace(document.TipoDocLetra))
        {
            parts.Add(document.TipoDocLetra);
        }

        if (!string.IsNullOrWhiteSpace(document.TipoDocDescripcion))
        {
            parts.Add(document.TipoDocDescripcion);
        }

        return string.Join(" - ", parts);
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
            return "badge badge-outline-warning rounded-pill";
        }

        return estado.Trim().ToUpperInvariant() switch
        {
            "RECHAZO PRECARGA" => "badge badge-outline-danger rounded-pill",
            "PENDIENTE PRECARGA" => "badge badge-outline-danger rounded-pill",
            "PRECARGA PENDIENTE" => "badge badge-outline-danger rounded-pill",
            "PAGADO" => "badge badge-outline-success rounded-pill",
            "PAGO EMITIDO" => "badge badge-outline-success rounded-pill",
            // Agrega más estados según tu dominio
            _ => "badge badge-outline-warning rounded-pill"
        };
    }

    /// <summary>
    /// Handles the document click event to open the document details modal.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <returns></returns>
    private async Task OpenPaidDocumentDetails(int docId)
    {
        _isModalLoading = true;
        _selectedDocument = null;
        _selectedPdfFileName = string.Empty;

        // Ensure Toast is initialized before opening modal
        if (_toast is not null)
        {
            ToastService.SetToast(_toast);
        }

        StateHasChanged();

        await GetPaidDocumentWithDetails(docId);

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
    /// Retrieves the paid document details based on the document ID.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <returns></returns>
    private async Task GetPaidDocumentWithDetails(int docId)
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
    /// Handles the confirm payment action for a document.
    /// </summary>
    /// <param name="document">The document to confirm payment for.</param>
    /// <returns></returns>
    private async Task ConfirmarPago(DocumentResponse document)
    {
        try
        {
            await ShowToast($"Confirmando el pago del documento #{document.DocId}...", ToastType.Info);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al confirmar el pago:", ex.Message);
            await ShowToast("Error al confirmar el pago del documento.");
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
    /// Forces the date inputs to lose focus.
    /// </summary>
    /// <returns></returns>
    private async Task ForceDateInputsBlurAsync()
    {
        await JsRuntime.InvokeVoidAsync("blurElementById", "dateFrom");
        await JsRuntime.InvokeVoidAsync("blurElementById", "dateTo");
    }

    /// <summary>
    /// Displays a toast message using the toast service.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of toast (default: Error).</param>
    /// <returns></returns>
    private async Task ShowToast(string message, ToastType type = ToastType.Error)
    {
        try
        {
            // Ensure Toast is initialized before showing message
            if (_toast is not null)
            {
                ToastService.SetToast(_toast);
            }

            await ToastService.ShowAsync(message, type);
        }
        catch (InvalidOperationException)
        {
            // Toast not initialized - log to console as fallback
            try
            {
                await JsRuntime.InvokeVoidAsync("console.warn", $"Toast no disponible: {message}");
            }
            catch
            {
                // Silently fail to prevent breaking the app
            }
        }
        catch (Exception ex)
        {
            // Log any other errors but don't break the app
            try
            {
                await JsRuntime.InvokeVoidAsync("console.error", $"Error al mostrar toast: {ex.Message}. Mensaje original: {message}");
            }
            catch
            {
                // Silently fail to prevent breaking the app
            }
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
