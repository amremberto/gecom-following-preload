
using System.Globalization;
using System.Security.Claims;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Documents.Update;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.WebApp.Components.Modals;
using GeCom.Following.Preload.WebApp.Components.Shared;
using GeCom.Following.Preload.WebApp.Enums;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Documents;

public partial class Pending : IAsyncDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private ICurrencyService CurrencyService { get; set; } = default!;
    [Inject] private IDocumentTypeService DocumentTypeService { get; set; } = default!;
    [Inject] private ISapProviderSocietyService SapProviderSocietyService { get; set; } = default!;
    [Inject] private IProviderService ProviderService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isDataTableLoading;
    private bool _isModalLoading;

    private IJSObjectReference? _tableDatatableModule;
    private IJSObjectReference? _formWizardModule;
    private PreloadDocumentModal? _preloadModal;
    private Toast? _toast;
    private DotNetObjectReference<Pending>? _dotNetObjectReference;

    private bool _isProvider;
    private bool _isSociety;
    private string? _providerCuit;
    private string? _userEmail;
    private bool _hasReadOnlyRole;
    private bool _hasSupportedRole;
    private IEnumerable<DocumentResponse> _pendingDocuments = [];
    private DocumentResponse? _selectedDocument;
    private string _selectedPdfFileName = string.Empty;
    private IEnumerable<CurrencyResponse> _currencies = [];
    private string? _selectedCurrencyCode;
    private IEnumerable<DocumentTypeResponse> _documentTypes = [];
    private int? _selectedDocumentTypeId;
    private DateOnly? _selectedFechaFactura;
    private DateOnly? _selectedVencimientoCaecai;
    private IEnumerable<SocietySelectItemResponse> _societies = []; // Unified list of societies (always loaded)
    private IEnumerable<ProviderSelectItemResponse> _availableProviders = []; // For Society role users
    private string? _selectedSocietyCuit;
    private string? _selectedProviderCuit;
#pragma warning disable S4487 // Unread private fields - Used in Razor @bind directive
    private string _selectedPuntoDeVenta = string.Empty;
    private string _selectedNumeroComprobante = string.Empty;
    private string _selectedCaecai = string.Empty;
    private decimal? _selectedMontoBruto;
#pragma warning restore S4487

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

                // Initialize toast service with component reference
                ToastService.SetToast(_toast);

                // Check if user has ReadOnly role to hide certain UI elements
                await HasReadOnlyRoleAsync();

                // Check if user is a provider and get their CUIT
                // This must be done here, not in OnInitializedAsync, because it uses JavaScript interop
                // which is not available during pre-rendering
                await CheckIfProviderAndGetCuitAsync();

                // Check if user is a society and get their email
                // This must be done here, not in OnInitializedAsync, because it uses JavaScript interop
                // which is not available during pre-rendering
                await CheckIfSocietyAndGetEmailAsync();

                // Check if user has a supported role (must be done here, not in OnInitializedAsync
                // because JavaScript interop is not available during pre-rendering)
                await HasSupportedRoleAsync();

                StateHasChanged();

                _tableDatatableModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/components/table-datatable.min.js");
                _formWizardModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/components/form-wizard.min.js");
                await InvokeAsync(StateHasChanged); // Fuerza renderizado


                // If user has a supported role (Provider, Societies, Administrator, or ReadOnly), automatically load documents
                // The new endpoint will handle filtering automatically based on role
                if (UserHasSupportedRole())
                {
                    try
                    {
                        _isDataTableLoading = true;
                        StateHasChanged();

                        await JsRuntime.InvokeVoidAsync("destroyDataTable", "pending-documents-datatable");
                        await GetPendingDocuments();
                        await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");
                    }
                    catch (Exception ex)
                    {
                        await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar documentos pendientes:", ex.Message);
                        await ShowToast("Ocurrió un error al cargar los documentos pendientes. Por favor, intente nuevamente más tarde.");
                    }
                    finally
                    {
                        _isDataTableLoading = false;
                        StateHasChanged();
                    }
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");
                    await ShowToast("No tiene los permisos necesarios para ver los documentos pendientes. Por favor, contacte al administrador del sistema.", ToastType.Warning);
                }

                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");

                // Check if there's a document ID in the query string to open edit modal
                await CheckAndOpenEditModalFromQueryString();

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            // Ensure loading states are reset on error
            _isLoading = false;
            await JsRuntime.InvokeVoidAsync("console.error", "Error en Documents.OnAfterRenderAsync:", ex.Message);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Checks the query string for editDocId parameter and opens the edit modal if found.
    /// </summary>
    /// <returns></returns>
    private async Task CheckAndOpenEditModalFromQueryString()
    {
        try
        {
            Uri? uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            Dictionary<string, StringValues>? queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            if (queryParams.TryGetValue("editDocId", out StringValues editDocIdValue) &&
                int.TryParse(editDocIdValue.ToString(), out int docId))
            {
                // Show success message (Toast is already initialized at this point)
                await ShowToast($"Documento #{docId} precargado exitosamente.", ToastType.Success);

                // Remove the query parameter from the URL to clean it up
                var uriBuilder = new UriBuilder(uri)
                {
                    Query = string.Empty
                };
                NavigationManager.NavigateTo(uriBuilder.Uri.PathAndQuery, replace: true);

                // Ensure the grid is loaded with the latest data (including the newly created document)
                if (UserHasSupportedRole())
                {
                    _isDataTableLoading = true;
                    StateHasChanged();

                    await JsRuntime.InvokeVoidAsync("destroyDataTable", "pending-documents-datatable");
                    await GetPendingDocuments();
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");

                    _isDataTableLoading = false;
                    StateHasChanged();

                    // Wait a bit to ensure the data table is fully rendered
                    await Task.Delay(300);
                }

                // Find the document in the pending documents list
                DocumentResponse? documentToEdit = _pendingDocuments.FirstOrDefault(d => d.DocId == docId);
                if (documentToEdit is not null)
                {
                    // Open the edit modal for the document using the document from the dataTable
                    await OpenDocumentEdit(documentToEdit);
                }
                else
                {
                    await ShowToast("No se pudo encontrar el documento para editar.", ToastType.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al verificar parámetro editDocId:", ex.Message);
        }
    }

    private async Task GetPendingDocuments()
    {
        try
        {
            IEnumerable<DocumentResponse>? response;

            // Check if user is a provider - Providers need to use a different endpoint
            if (_isProvider && !string.IsNullOrWhiteSpace(_providerCuit))
            {
                // Providers: Use the endpoint that requires provider CUIT
                response = await DocumentService.GetPendingDocumentsByProviderAsync(
                    _providerCuit,
                    cancellationToken: default);
            }
            else
            {
                // For other roles (Administrator, ReadOnly, Societies), use the unified endpoint
                // The backend will automatically filter based on the user's role:
                // - Societies: Filters by all societies assigned to the user
                // - Administrator/ReadOnly: Returns all pending documents
                response = await DocumentService.GetPendingDocumentsAsync(cancellationToken: default);
            }

            _pendingDocuments = response ?? [];
        }
        catch (ApiRequestException httpEx)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos pendientes:", httpEx.Message);
            await ShowToast(httpEx.Message);
            _pendingDocuments = [];
        }
        catch (UnauthorizedAccessException)
        {
            await ShowToast("No tiene autorización para realizar esta operación. Por favor, inicie sesión nuevamente.");
            _pendingDocuments = [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al buscar documentos pendientes:", ex.Message);
            await ShowToast("Ocurrió un error al buscar los documentos pendientes. Por favor, intente nuevamente más tarde.");
            _pendingDocuments = [];
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task ShowToast(string message, ToastType type = ToastType.Error)
    {
        await ToastService.ShowAsync(message, type);
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
    /// Checks if the document can be deleted based on status and user role.
    /// </summary>
    /// <param name="document">The document to check.</param>
    /// <returns>True if the document can be deleted, false otherwise.</returns>
    private bool CanDeleteDocument(DocumentResponse document)
    {
        bool hasCorrectStatus =
            document.EstadoDescripcion?.Trim().ToUpperInvariant() == "PRECARGA PENDIENTE" ||
            document.EstadoDescripcion?.Trim().ToUpperInvariant() == "PENDIENTE PRECARGA";
        bool canDelete = hasCorrectStatus && _hasSupportedRole;

        return canDelete;
    }

    /// <summary>
    /// Checks if the document is in "Pendiente Precarga" status.
    /// </summary>
    /// <param name="document">The document to check.</param>
    /// <returns>True if the document is in "Pendiente Precarga" status, false otherwise.</returns>
    private static bool IsPendingPreloadStatus(DocumentResponse? document)
    {
        if (document is null || string.IsNullOrWhiteSpace(document.EstadoDescripcion))
        {
            return false;
        }

        string estado = document.EstadoDescripcion.Trim().ToUpperInvariant();
        return estado == "PENDIENTE PRECARGA";
    }

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
    /// Handles the edit document action.
    /// </summary>
    /// <param name="document">The document to edit.</param>
    /// <returns></returns>
    private async Task EditDocument(DocumentResponse document)
    {
        await OpenDocumentEdit(document);
    }

    /// <summary>
    /// Opens the document edit modal.
    /// </summary>
    /// <param name="document">The document to edit. If provided, it will be used directly without querying the database.</param>
    /// <returns></returns>
    private async Task OpenDocumentEdit(DocumentResponse document)
    {
        ArgumentNullException.ThrowIfNull(document);

        _isModalLoading = true;
        _selectedDocument = null;
        _selectedPdfFileName = string.Empty;
        _selectedCurrencyCode = null;
        _selectedDocumentTypeId = null;
        _selectedFechaFactura = null;
        _selectedVencimientoCaecai = null;
        _selectedSocietyCuit = null;
        _selectedPuntoDeVenta = string.Empty;
        _selectedNumeroComprobante = string.Empty;
        _selectedCaecai = string.Empty;
        _selectedMontoBruto = null;
        StateHasChanged();

        // Note: Role verification (_isProvider, _providerCuit, _isSociety, _userEmail) 
        // is already done in OnAfterRenderAsync when the page loads, so we don't need to verify again here.
        await JsRuntime.InvokeVoidAsync("console.log", $"[OpenDocumentEdit] Usando roles ya verificados. _isProvider: {_isProvider}, _providerCuit: {_providerCuit}, _isSociety: {_isSociety}, _userEmail: {_userEmail}");

        // Use the document passed directly from the dataTable for basic data
        // However, if attachments are needed (for PDF display), we need to load them
        _selectedDocument = document;
        await JsRuntime.InvokeVoidAsync("console.log", $"[OpenDocumentEdit] Usando documento del dataTable. DocId: {document.DocId}");

        // Set document values FIRST before loading dropdowns
        if (_selectedDocument is not null)
        {
            // Set the selected currency code from the document
            _selectedCurrencyCode = _selectedDocument.Moneda;
            // Set the selected document type ID from the document
            _selectedDocumentTypeId = _selectedDocument.TipoDocId;
            // Set the selected dates from the document
            _selectedFechaFactura = _selectedDocument.FechaEmisionComprobante;
            _selectedVencimientoCaecai = _selectedDocument.VencimientoCaecai;
            // Set the selected society CUIT from the document
            _selectedSocietyCuit = _selectedDocument.SociedadCuit;
            // Set the selected provider CUIT from the document (for Society role users)
            if (_isSociety)
            {
                _selectedProviderCuit = _selectedDocument.ProveedorCuit;
            }
            // Set the selected text fields from the document
            _selectedPuntoDeVenta = _selectedDocument.PuntoDeVenta ?? string.Empty;
            _selectedNumeroComprobante = _selectedDocument.NumeroComprobante ?? string.Empty;
            _selectedCaecai = _selectedDocument.Caecai ?? string.Empty;
            _selectedMontoBruto = _selectedDocument.MontoBruto;

            await JsRuntime.InvokeVoidAsync("console.log", $"[OpenDocumentEdit] Valores del documento establecidos. SociedadCuit: {_selectedDocument.SociedadCuit}, ProveedorCuit: {_selectedDocument.ProveedorCuit}");
        }

        await LoadCurrencies();
        await LoadDocumentTypes();
        await LoadSocieties(); // Always load societies (like currencies and document types)

        // If Society role, handle provider loading based on document's society
        if (_isSociety && !string.IsNullOrWhiteSpace(_userEmail))
        {
            // Initialize providers list as empty first
            _availableProviders = [];

            // If document has a society, load providers for that society
            string? societyCuitToLoad = _selectedDocument?.SociedadCuit ?? _selectedSocietyCuit;
            if (!string.IsNullOrWhiteSpace(societyCuitToLoad))
            {
                await LoadProvidersBySociety(societyCuitToLoad);
                await JsRuntime.InvokeVoidAsync("console.log", $"[OpenDocumentEdit] Proveedores cargados para sociedad {societyCuitToLoad}: {_availableProviders.Count()}");
            }
            else
            {
                // Document has no society, keep providers list empty
                await JsRuntime.InvokeVoidAsync("console.log", "[OpenDocumentEdit] Documento sin sociedad, lista de proveedores vacía");
            }
        }

        _isModalLoading = false;
        StateHasChanged();
        await JsRuntime.InvokeVoidAsync("console.log", $"[OpenDocumentEdit] StateHasChanged llamado. _isProvider: {_isProvider}, _isSociety: {_isSociety}, _userEmail: {_userEmail}, _selectedSocietyCuit: {_selectedSocietyCuit}, _selectedProviderCuit: {_selectedProviderCuit}");
        await JsRuntime.InvokeVoidAsync("console.log", $"[OpenDocumentEdit] Listas disponibles. _societies: {_societies.Count()}, _availableProviders: {_availableProviders.Count()}");

        if (_selectedDocument is not null)
        {
            // Create DotNetObjectReference for callback if not exists
            _dotNetObjectReference?.Dispose();
            _dotNetObjectReference = DotNetObjectReference.Create(this);

            // Register a global JavaScript function to store the DotNetObjectReference
            await JsRuntime.InvokeVoidAsync("eval", @"
                (function() {
                    window.setEditDocumentDotNetRef = function(dotNetRef) {
                        window.editDocumentDotNetRef = dotNetRef;
                    };
                })();
            ");

            // Store the DotNetObjectReference in JavaScript
            await JsRuntime.InvokeVoidAsync("setEditDocumentDotNetRef", _dotNetObjectReference);

            // Show edit modal and set up cleanup on close
            await JsRuntime.InvokeVoidAsync("eval", @"
                (function() {
                    var modalElement = document.getElementById('edit-document-modal');
                    var modal = new bootstrap.Modal(modalElement);
                    
                    // Initialize SELECT2 and Flatpickr after modal is fully shown
                    modalElement.addEventListener('shown.bs.modal', function() {
                        // Use setTimeout to ensure DOM is completely ready, even if PDF fails to load
                        setTimeout(function() {
                            // Call C# method to initialize SELECT2 and Flatpickr
                            if (window.editDocumentDotNetRef) {
                                window.editDocumentDotNetRef.invokeMethodAsync('InitializeEditFormComponents');
                            }
                        }, 300);
                    }, { once: true });
                    
                    // Clean up SELECT2, Flatpickr, and Wizard when modal is hidden
                    modalElement.addEventListener('hidden.bs.modal', function() {
                        // Clean up SELECT2
                        var select = $('#currency-select-edit');
                        if (select.data('select2')) {
                            select.select2('destroy');
                        }
                        var selectDocType = $('#document-type-select-edit');
                        if (selectDocType.data('select2')) {
                            selectDocType.select2('destroy');
                        }
                        var selectSociety = $('#society-select-edit');
                        if (selectSociety.data('select2')) {
                            selectSociety.select2('destroy');
                        }
                        var selectProvider = $('#provider-select-edit');
                        if (selectProvider.data('select2')) {
                            selectProvider.select2('destroy');
                        }
                        
                        // Clean up Flatpickr
                        var fechaFacturaInput = document.getElementById('fecha-factura-edit');
                        if (fechaFacturaInput && fechaFacturaInput._flatpickr) {
                            fechaFacturaInput._flatpickr.destroy();
                        }
                        var vencCaecaiInput = document.getElementById('venc-caecai-edit');
                        if (vencCaecaiInput && vencCaecaiInput._flatpickr) {
                            vencCaecaiInput._flatpickr.destroy();
                        }
                        
                        // Reset wizard to first step
                        var wizardElement = document.getElementById('edit-document-wizard');
                        if (wizardElement) {
                            var firstTab = wizardElement.querySelector('.nav-link');
                            if (firstTab) {
                                var firstTabPane = document.querySelector(firstTab.getAttribute('href'));
                                if (firstTabPane) {
                                    // Reset all tabs
                                    wizardElement.querySelectorAll('.nav-link').forEach(function(link) {
                                        link.classList.remove('active');
                                    });
                                    wizardElement.querySelectorAll('.tab-pane').forEach(function(pane) {
                                        pane.classList.remove('show', 'active');
                                    });
                                    // Activate first tab
                                    firstTab.classList.add('active');
                                    firstTabPane.classList.add('show', 'active');
                                }
                            }
                        }
                        
                        // Clean up DotNet reference
                        if (window.editDocumentDotNetRef) {
                            window.editDocumentDotNetRef = null;
                        }
                    }, { once: true });
                    
                    modal.show();
                })();
            ");
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al abrir el modal de edición del documento: respuesta nula");
            await ShowToast("Error al cargar el documento para editar.");
        }
    }

    /// <summary>
    /// Loads all currencies from the API.
    /// </summary>
    /// <returns></returns>
    private async Task LoadCurrencies()
    {
        try
        {
            IEnumerable<CurrencyResponse>? response = await CurrencyService.GetAllAsync(cancellationToken: default);
            _currencies = response ?? [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar monedas:", ex.Message);
            await ShowToast("Error al cargar las monedas disponibles.");
            _currencies = [];
        }
    }

    /// <summary>
    /// Initializes SELECT2 for the currency dropdown.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeCurrencySelect2()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("initCurrencySelect2", _selectedCurrencyCode);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar SELECT2:", ex.Message);
        }
    }

    /// <summary>
    /// Handles the currency selection change.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns></returns>
    private Task OnCurrencyChanged(ChangeEventArgs e)
    {
        _selectedCurrencyCode = e.Value?.ToString();
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads all document types from the API.
    /// </summary>
    /// <returns></returns>
    private async Task LoadDocumentTypes()
    {
        try
        {
            IEnumerable<DocumentTypeResponse>? response = await DocumentTypeService.GetAllAsync(cancellationToken: default);
            _documentTypes = response ?? [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar tipos de documento:", ex.Message);
            await ShowToast("Error al cargar los tipos de documento disponibles.");
            _documentTypes = [];
        }
    }

    /// <summary>
    /// Handles the document type selection change.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns></returns>
    private Task OnDocumentTypeChanged(ChangeEventArgs e)
    {
        _selectedDocumentTypeId = int.TryParse(e.Value?.ToString(), out int tipoDocId) ? tipoDocId : null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes SELECT2 for the document type dropdown.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeDocumentTypeSelect2()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("initDocumentTypeSelect2", _selectedDocumentTypeId?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar SELECT2 para tipo de documento:", ex.Message);
        }
    }

    /// <summary>
    /// Loads all societies based on user role. Always called (like LoadCurrencies and LoadDocumentTypes).
    /// Uses a unified endpoint that automatically determines which societies to return based on the authenticated user's role.
    /// </summary>
    /// <returns></returns>
    private async Task LoadSocieties()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("console.log", $"[LoadSocieties] Iniciando carga de sociedades usando endpoint unificado.");

            // Use unified endpoint that determines societies based on user role automatically
            IEnumerable<SocietySelectItemResponse>? response = await SapProviderSocietyService.GetSocietiesForCurrentUserAsync(
                cancellationToken: default);
            _societies = response ?? [];

            await JsRuntime.InvokeVoidAsync("console.log", $"[LoadSocieties] Sociedades cargadas: {_societies.Count()}");
            if (_societies.Any())
            {
                await JsRuntime.InvokeVoidAsync("console.log", $"[LoadSocieties] Primeras sociedades: {string.Join(", ", _societies.Take(3).Select(s => s.Cuit))}");
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"Error al cargar sociedades: {ex.Message}");
            await JsRuntime.InvokeVoidAsync("console.error", $"Stack trace: {ex.StackTrace}");
            await ShowToast("Error al cargar las sociedades disponibles.");
            _societies = [];
        }
    }

    /// <summary>
    /// Loads all providers that can assign documents to a specific society (for Society role users).
    /// </summary>
    /// <param name="societyCuit">The society CUIT.</param>
    /// <returns></returns>
    private async Task LoadProvidersBySociety(string societyCuit)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(societyCuit))
            {
                _availableProviders = [];
                return;
            }

            // Get providers for select dropdown directly from the API
            IEnumerable<ProviderSelectItemResponse>? providers = await SapProviderSocietyService.GetProvidersBySocietyCuitForSelectAsync(
                societyCuit,
                cancellationToken: default);

            _availableProviders = providers ?? [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar proveedores por sociedad:", ex.Message);
            await ShowToast("Error al cargar los proveedores disponibles.");
            _availableProviders = [];
        }
    }

    /// <summary>
    /// Initializes SELECT2 for the society dropdown.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeSocietySelect2()
    {
        try
        {
            // Verify element exists before initializing
            bool elementExists = await JsRuntime.InvokeAsync<bool>("eval", @"
                (function() {
                    var selectElement = document.getElementById('society-select-edit');
                    return selectElement !== null && selectElement !== undefined;
                })();
            ");

            if (!elementExists)
            {
                await JsRuntime.InvokeVoidAsync("console.error", "No se puede inicializar SELECT2 para sociedad: el elemento 'society-select-edit' no existe en el DOM");
                return;
            }

            await JsRuntime.InvokeVoidAsync("initSocietySelect2", _selectedSocietyCuit);

            // Add event listener for SELECT2 change event to call C# method
            // Use a small delay to ensure SELECT2 is fully initialized
            if (_dotNetObjectReference is not null)
            {
                await Task.Delay(100); // Small delay to ensure SELECT2 is initialized
                await JsRuntime.InvokeVoidAsync("eval", @"
                    (function() {
                        var $select = $('#society-select-edit');
                        if ($select.length > 0 && $select.data('select2')) {
                            // Remove existing listeners to avoid duplicates
                            $select.off('change.select2.society');
                            
                            // Add listener for SELECT2 change event
                            $select.on('change.select2.society', function() {
                                var selectedValue = $(this).val();
                                if (window.editDocumentDotNetRef) {
                                    window.editDocumentDotNetRef.invokeMethodAsync('OnSocietyChangedFromSelect2', selectedValue || '');
                                }
                            });
                        } else {
                            console.warn('SELECT2 not initialized for society-select-edit');
                        }
                    })();
                ");
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar SELECT2 para cliente:", ex.Message);
        }
    }

    /// <summary>
    /// Initializes SELECT2 for the provider dropdown.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeProviderSelect2()
    {
        try
        {
            // Verify element exists before initializing
            bool elementExists = await JsRuntime.InvokeAsync<bool>("eval", @"
                (function() {
                    var selectElement = document.getElementById('provider-select-edit');
                    return selectElement !== null && selectElement !== undefined;
                })();
            ");

            if (!elementExists)
            {
                await JsRuntime.InvokeVoidAsync("console.error", "No se puede inicializar SELECT2 para proveedor: el elemento 'provider-select-edit' no existe en el DOM");
                return;
            }

            // Pass null or empty string explicitly if _selectedProviderCuit is null
            string? providerCuitToSet = _selectedProviderCuit ?? string.Empty;
            await JsRuntime.InvokeVoidAsync("initProviderSelect2", providerCuitToSet);

            // Add event listener for SELECT2 change event to call C# method (same as society)
            if (_dotNetObjectReference is not null)
            {
                await Task.Delay(100); // Small delay to ensure SELECT2 is initialized
                await JsRuntime.InvokeVoidAsync("eval", @"
                    (function() {
                        var $select = $('#provider-select-edit');
                        if ($select.length > 0 && $select.data('select2')) {
                            // Remove existing listeners to avoid duplicates
                            $select.off('change.select2.provider');
                            
                            // Add listener for SELECT2 change event
                            $select.on('change.select2.provider', function() {
                                var selectedValue = $(this).val();
                                if (window.editDocumentDotNetRef) {
                                    window.editDocumentDotNetRef.invokeMethodAsync('OnProviderChangedFromSelect2', selectedValue || '');
                                }
                            });
                        } else {
                            console.warn('SELECT2 not initialized for provider-select-edit');
                        }
                    })();
                ");
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar SELECT2 para proveedor:", ex.Message);
        }
    }

    /// <summary>
    /// Handles the society selection change from SELECT2 (called from JavaScript).
    /// </summary>
    /// <param name="societyCuit">The selected society CUIT.</param>
    /// <returns></returns>
    [JSInvokable]
    public async Task OnSocietyChangedFromSelect2(string societyCuit)
    {
        string? newSocietyCuit = string.IsNullOrWhiteSpace(societyCuit) ? null : societyCuit;
        await HandleSocietyChange(newSocietyCuit);
    }

    /// <summary>
    /// Handles the society selection change.
    /// When a society is selected, loads providers for that society.
    /// When society is cleared, clears the providers list.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns></returns>
    private async Task OnSocietyChanged(ChangeEventArgs e)
    {
        string? newSocietyCuit = e.Value?.ToString();
        await HandleSocietyChange(newSocietyCuit);
    }

    /// <summary>
    /// Common method to handle society change logic.
    /// </summary>
    /// <param name="newSocietyCuit">The new society CUIT.</param>
    /// <returns></returns>
    private async Task HandleSocietyChange(string? newSocietyCuit)
    {
        _selectedSocietyCuit = newSocietyCuit;

        // If user is Society role
        if (_isSociety && !string.IsNullOrWhiteSpace(_userEmail))
        {
            if (!string.IsNullOrWhiteSpace(newSocietyCuit))
            {
                // Society selected: Load providers for that society
                await LoadProvidersBySociety(newSocietyCuit);
                await JsRuntime.InvokeVoidAsync("console.log", $"[OnSocietyChanged] Proveedores cargados para sociedad {newSocietyCuit}: {_availableProviders.Count()}");

                // Reset selected provider when society changes (user must select a new provider)
                _selectedProviderCuit = null;

                // Update state and wait for DOM to update
                StateHasChanged();
                await Task.Delay(200); // Allow DOM to update with new provider options

                // Clear the data-selected-value attribute and reinitialize provider SELECT2 with null value
                await JsRuntime.InvokeVoidAsync("eval", @"
                    (function() {
                        var selectElement = document.getElementById('provider-select-edit');
                        if (selectElement) {
                            selectElement.removeAttribute('data-selected-value');
                        }
                    })();
                ");
                await InitializeProviderSelect2();
            }
            else
            {
                // Society cleared: Clear providers list
                _availableProviders = [];
                _selectedProviderCuit = null;
                await JsRuntime.InvokeVoidAsync("console.log", "[OnSocietyChanged] Sociedad deseleccionada, lista de proveedores vaciada");

                // Update state and wait for DOM to update
                StateHasChanged();
                await Task.Delay(200);

                // Clear the data-selected-value attribute and reinitialize provider SELECT2 with null value
                await JsRuntime.InvokeVoidAsync("eval", @"
                    (function() {
                        var selectElement = document.getElementById('provider-select-edit');
                        if (selectElement) {
                            selectElement.removeAttribute('data-selected-value');
                        }
                    })();
                ");
                await InitializeProviderSelect2();
            }
        }
        else
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles the provider selection change.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns></returns>
    /// <summary>
    /// Handles the provider selection change from SELECT2 (called from JavaScript).
    /// </summary>
    /// <param name="providerCuit">The selected provider CUIT.</param>
    /// <returns></returns>
    [JSInvokable]
    public Task OnProviderChangedFromSelect2(string providerCuit)
    {
        _selectedProviderCuit = string.IsNullOrWhiteSpace(providerCuit) ? null : providerCuit;
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the provider selection change.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns></returns>
    private Task OnProviderChanged(ChangeEventArgs e)
    {
        _selectedProviderCuit = e.Value?.ToString();
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes Flatpickr for the date fields.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeDatePickers()
    {
        try
        {
            // Get today's date for maxDate validation
            var today = DateOnly.FromDateTime(DateTime.Today);
            string? todayStr = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Initialize Fecha Factura with maxDate = today
            if (_selectedFechaFactura.HasValue)
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#fecha-factura-edit",
                    new
                    {
                        defaultDate = _selectedFechaFactura.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        maxDate = todayStr,
                        fieldId = "fecha-factura-edit"
                    });
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#fecha-factura-edit",
                    new
                    {
                        maxDate = todayStr,
                        fieldId = "fecha-factura-edit"
                    });
            }

            // Initialize Venc. CAE / CAI with minDate = fecha factura (if available)
            string? minDateStr = null;
            if (_selectedFechaFactura.HasValue)
            {
                minDateStr = _selectedFechaFactura.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (_selectedVencimientoCaecai.HasValue)
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#venc-caecai-edit",
                    new
                    {
                        defaultDate = _selectedVencimientoCaecai.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        minDate = minDateStr,
                        fieldId = "venc-caecai-edit",
                        dependsOnFieldId = "fecha-factura-edit"
                    });
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#venc-caecai-edit",
                    new
                    {
                        minDate = minDateStr,
                        fieldId = "venc-caecai-edit",
                        dependsOnFieldId = "fecha-factura-edit"
                    });
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar Flatpickr:", ex.Message);
        }
    }

    /// <summary>
    /// Shows a toast message from JavaScript. This method is called from JavaScript when validation fails.
    /// </summary>
    /// <param name="message">The message to display.</param>
    [JSInvokable]
    public async Task ShowValidationToast(string message)
    {
        await ShowToast(message, ToastType.Warning);
    }

    /// <summary>
    /// Shows a confirmation dialog using SweetAlert2 (similar to "With Confirm Button" component) and returns the user's choice.
    /// </summary>
    /// <param name="message">The message to display in the confirmation dialog.</param>
    /// <returns>True if user confirmed, false if cancelled.</returns>
    [JSInvokable]
    public async Task<bool> ShowConfirmationDialog(string message)
    {
        try
        {
            bool result = await JsRuntime.InvokeAsync<bool>("showSweetAlertConfirm",
                "Confirmar acción",
                message,
                "Aceptar",
                "Cancelar");
            return result;
        }
        catch (Exception)
        {
            // Fallback: retornar false si SweetAlert no está disponible
            return false;
        }
    }

    /// <summary>
    /// Initializes SELECT2 and Flatpickr components for the edit document form.
    /// This method is called from JavaScript after the modal is fully shown.
    /// </summary>
    /// <returns></returns>
    [JSInvokable]
    public async Task InitializeEditFormComponents()
    {
        try
        {
            // Ensure DOM is updated with current values
            StateHasChanged();
            await Task.Delay(300); // Delay to ensure DOM update is complete (same as before)

            // Initialize SELECT2 components (always initialize, like currencies and document types)
            await InitializeCurrencySelect2();
            await InitializeDocumentTypeSelect2();
            await InitializeSocietySelect2(); // Always initialize (like currencies and document types)

            // Initialize provider SELECT2 if user is Society role
            if (_isSociety && !string.IsNullOrWhiteSpace(_userEmail))
            {
                await InitializeProviderSelect2();
            }

            // Initialize date pickers
            await InitializeDatePickers();

            // Initialize wizard with validation
            await InitializeEditDocumentWizard();
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar componentes del formulario:", ex.Message);
        }
    }

    /// <summary>
    /// Updates the document from JavaScript when user clicks "Siguiente" with changes.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <param name="sociedadCuit">The sociedad CUIT.</param>
    /// <param name="proveedorCuit">The proveedor CUIT.</param>
    /// <param name="tipoDocId">The document type ID.</param>
    /// <param name="puntoDeVenta">The punto de venta.</param>
    /// <param name="numeroComprobante">The número comprobante.</param>
    /// <param name="fechaEmisionComprobante">The fecha emisión comprobante (format: yyyy-MM-dd).</param>
    /// <param name="moneda">The moneda.</param>
    /// <param name="montoBruto">The monto bruto.</param>
    /// <param name="caecai">The CAE/CAI.</param>
    /// <param name="vencimientoCaecai">The vencimiento CAE/CAI (format: yyyy-MM-dd).</param>
    /// <returns>An object with success status and message.</returns>
    [JSInvokable]
    public async Task<UpdateDocumentResult> UpdateDocumentFromWizard(
        int docId,
        string? sociedadCuit,
        int? tipoDocId,
        string? puntoDeVenta,
        string? numeroComprobante,
        string? fechaEmisionComprobante,
        string? moneda,
        decimal? montoBruto,
        string? caecai,
        string? vencimientoCaecai)
    {
        try
        {
            if (_selectedDocument is null)
            {
                return new UpdateDocumentResult
                {
                    Success = false,
                    Message = "No hay documento seleccionado."
                };
            }

            // Parse dates
            DateOnly? fechaEmision = null;
            if (!string.IsNullOrWhiteSpace(fechaEmisionComprobante) &&
                DateOnly.TryParse(fechaEmisionComprobante, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateOnly fechaEmisionParsed))
            {
                fechaEmision = fechaEmisionParsed;
            }

            DateOnly? vencimiento = null;
            if (!string.IsNullOrWhiteSpace(vencimientoCaecai) &&
                DateOnly.TryParse(vencimientoCaecai, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateOnly vencimientoParsed))
            {
                vencimiento = vencimientoParsed;
            }

            // Determine provider CUIT based on user role (same pattern as sociedadCuit)
            // Use _selectedProviderCuit which is updated when SELECT2 changes
            string? proveedorCuitFinal;
            if (_isProvider && !string.IsNullOrWhiteSpace(_providerCuit))
            {
                // Provider role: use the logged-in provider's CUIT
                proveedorCuitFinal = _providerCuit;
            }
            else if (_isSociety && !string.IsNullOrWhiteSpace(_selectedProviderCuit))
            {
                // Society role: use the selected provider from component state (updated by SELECT2 listener)
                proveedorCuitFinal = _selectedProviderCuit;
            }
            else
            {
                // Fallback: use document's provider
                proveedorCuitFinal = _selectedDocument.ProveedorCuit;
            }

            // Build update request
            UpdateDocumentRequest request = new(
                DocId: docId,
                ProveedorCuit: proveedorCuitFinal,
                SociedadCuit: sociedadCuit,
                TipoDocId: tipoDocId,
                PuntoDeVenta: puntoDeVenta,
                NumeroComprobante: numeroComprobante,
                FechaEmisionComprobante: fechaEmision,
                Moneda: moneda,
                MontoBruto: montoBruto,
                CodigoDeBarras: null,
                Caecai: caecai,
                VencimientoCaecai: vencimiento,
                EstadoId: null,
                NombreSolicitante: null);

            // Call update service
            DocumentResponse? updatedDocument = await DocumentService.UpdateAsync(docId, request, default);

            if (updatedDocument is null)
            {
                return new UpdateDocumentResult
                {
                    Success = false,
                    Message = "Error al actualizar el documento. No se recibió respuesta del servidor."
                };
            }

            // Update selected document with new data
            _selectedDocument = updatedDocument;

            // Update form fields with new values
            _selectedCurrencyCode = updatedDocument.Moneda;
            _selectedDocumentTypeId = updatedDocument.TipoDocId;
            _selectedFechaFactura = updatedDocument.FechaEmisionComprobante;
            _selectedVencimientoCaecai = updatedDocument.VencimientoCaecai;
            _selectedSocietyCuit = updatedDocument.SociedadCuit;
            _selectedProviderCuit = updatedDocument.ProveedorCuit; // Update provider CUIT from updated document
            _selectedPuntoDeVenta = updatedDocument.PuntoDeVenta ?? string.Empty;
            _selectedNumeroComprobante = updatedDocument.NumeroComprobante ?? string.Empty;
            _selectedCaecai = updatedDocument.Caecai ?? string.Empty;
            _selectedMontoBruto = updatedDocument.MontoBruto;

            // Update the document in the _pendingDocuments collection to reflect changes in the dataTable
            await UpdateDocumentInPendingList(updatedDocument);

            // Update SELECT2 components with new values
            await InitializeProviderSelect2();

            StateHasChanged();

            return new UpdateDocumentResult
            {
                Success = true,
                Message = "Documento actualizado exitosamente."
            };
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al actualizar documento:", ex.Message);
            return new UpdateDocumentResult
            {
                Success = false,
                Message = $"Error al actualizar el documento: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Updates the document in the _pendingDocuments collection and refreshes the dataTable.
    /// </summary>
    /// <param name="updatedDocument">The updated document.</param>
    /// <returns></returns>
    private async Task UpdateDocumentInPendingList(DocumentResponse updatedDocument)
    {
        try
        {
            // Find the document in the pending documents list and replace it
            var pendingList = _pendingDocuments.ToList();
            int index = pendingList.FindIndex(d => d.DocId == updatedDocument.DocId);

            if (index >= 0)
            {
                // Replace the old document with the updated one
                pendingList[index] = updatedDocument;
                _pendingDocuments = pendingList;

                // Refresh the dataTable to show the updated data
                await JsRuntime.InvokeVoidAsync("destroyDataTable", "pending-documents-datatable");
                StateHasChanged();
                await Task.Delay(100); // Small delay to ensure DOM is updated
                await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");

                await JsRuntime.InvokeVoidAsync("console.log", $"Documento #{updatedDocument.DocId} actualizado en el dataTable.");
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("console.warn", $"No se encontró el documento #{updatedDocument.DocId} en la lista de documentos pendientes.");
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"Error al actualizar el documento en la lista: {ex.Message}");
        }
    }

    /// <summary>
    /// Result object for UpdateDocumentFromWizard method.
    /// </summary>
    public class UpdateDocumentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Initializes the edit document wizard with validation.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeEditDocumentWizard()
    {
        try
        {
            bool isPendingPreload = _selectedDocument is not null && IsPendingPreloadStatus(_selectedDocument);

            // Create DotNetObjectReference if it doesn't exist (it should already exist from OpenDocumentEdit)
            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            int docId = _selectedDocument?.DocId ?? 0;
            await JsRuntime.InvokeVoidAsync("initEditDocumentWizard", isPendingPreload, _dotNetObjectReference, docId);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar el wizard:", ex.Message);
        }
    }

    /// <summary>
    /// Cleans up SELECT2, Flatpickr, and Wizard when the modal is closed.
    /// </summary>
    /// <returns></returns>
    private async Task CleanupCurrencySelect2()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("cleanupSelect2InModal");
            await JsRuntime.InvokeVoidAsync("cleanupFlatpickrInModal");
            await JsRuntime.InvokeVoidAsync("resetEditDocumentWizard");
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al limpiar SELECT2 y Flatpickr:", ex.Message);
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
            await ShowToast($"Funcionalidad de eliminación para el documento {document.DocId} pendiente de implementar.", ToastType.Info);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al eliminar documento:", ex.Message);
            await ShowToast("Error al intentar eliminar el documento.");
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
               _selectedDocument.Attachments?
                .Any(a => a.FechaBorrado is null) == true;
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
    /// Handles the document preloaded event from the modal.
    /// </summary>
    /// <param name="document">The preloaded document.</param>
    /// <returns></returns>
    private async Task OnDocumentPreloaded(DocumentResponse document)
    {
        try
        {
            await ShowToast($"Documento #{document.DocId} precargado exitosamente.", ToastType.Success);

            // Recargar la grilla de documentos pendientes
            _isDataTableLoading = true;
            StateHasChanged();

            await JsRuntime.InvokeVoidAsync("destroyDataTable", "pending-documents-datatable");
            await GetPendingDocuments();
            await JsRuntime.InvokeVoidAsync("loadDataTable", "pending-documents-datatable");

            _isDataTableLoading = false;
            StateHasChanged();

            // Abrir el modal de edición del documento
            // Find the document in the pending documents list (it should be there after reload)
            DocumentResponse? documentToEdit = _pendingDocuments.FirstOrDefault(d => d.DocId == document.DocId);
            if (documentToEdit is not null)
            {
                await OpenDocumentEdit(documentToEdit);
            }
            else
            {
                // Fallback: use the document passed as parameter if not found in the list
                await OpenDocumentEdit(document);
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al procesar el documento precargado:", ex.Message);
            await ShowToast("Error al procesar el documento creado.");
            _isDataTableLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Checks if the current user has the ReadOnly role.
    /// </summary>
    private async Task HasReadOnlyRoleAsync()
    {
        try
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            if (user is null)
            {
                await JsRuntime.InvokeVoidAsync("console.log", "[HasReadOnlyRoleAsync] User is null");
                _hasReadOnlyRole = false;
                return;
            }

            _hasReadOnlyRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadReadOnly);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"[HasReadOnlyRoleAsync] Error: {ex.Message}");
            _hasReadOnlyRole = false;
        }
    }

    /// <summary>
    /// Checks if the current user has a supported role for document access.
    /// Supported roles: Administrator, Societies, Providers, or ReadOnly.
    /// Roles should be mapped during authentication, so we only need to check claims.
    /// </summary>
    /// <returns>True if the user has one of the supported roles, false otherwise.</returns>
    private async Task HasSupportedRoleAsync()
    {
        try
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            if (user is null)
            {
                await JsRuntime.InvokeVoidAsync("console.log", "[HasSupportedRoleAsync] User is null");
                _hasSupportedRole = false;
                return;
            }

            // Check for supported roles that can access documents
            string[] targetRoles =
            [
                AuthorizationConstants.Roles.FollowingAdministrator,
            AuthorizationConstants.Roles.FollowingPreloadSocieties,
            AuthorizationConstants.Roles.FollowingPreloadProviders,
            AuthorizationConstants.Roles.FollowingPreloadReadOnly
            ];

            _hasSupportedRole = targetRoles.Any(role => user.IsInRole(role));
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"[HasSupportedRoleAsync] Error: {ex.Message}");
            _hasSupportedRole = false;
        }
    }

    /// <summary>
    /// Checks if the current user is a Society and gets their email from the claim.
    /// </summary>
    /// <returns></returns>
    private async Task CheckIfSocietyAndGetEmailAsync()
    {
        try
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal? user = authState.User;

            if (user is null)
            {
                _isSociety = false;
                _userEmail = null;
                return;
            }

            // Check if user has the society role
            bool hasSocietyRole = user.IsInRole(AuthorizationConstants.Roles.FollowingPreloadSocieties) ||
                user.HasClaim(ClaimTypes.Role, AuthorizationConstants.Roles.FollowingPreloadSocieties) ||
                user.HasClaim(AuthorizationConstants.RoleClaimType, AuthorizationConstants.Roles.FollowingPreloadSocieties);

            if (hasSocietyRole)
            {
                _isSociety = true;

                // Get email from claim
                Claim? emailClaim = user.FindFirst(ClaimTypes.Email) ?? user.FindFirst("email");
                if (emailClaim is not null && !string.IsNullOrWhiteSpace(emailClaim.Value))
                {
                    _userEmail = emailClaim.Value;
                    await JsRuntime.InvokeVoidAsync("console.log", $"[CheckIfSocietyAndGetEmailAsync] User is a society with email: {_userEmail}");
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync("console.warn", "[CheckIfSocietyAndGetEmailAsync] User has society role but no email claim found");
                    _userEmail = null;
                }
            }
            else
            {
                _isSociety = false;
                _userEmail = null;
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", $"[CheckIfSocietyAndGetEmailAsync] Error: {ex.Message}");
            _isSociety = false;
            _userEmail = null;
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

    private bool UserHasSupportedRole()
    {
        return _isProvider || _hasSupportedRole;
    }

    public async ValueTask DisposeAsync()
    {
        if (_tableDatatableModule is not null)
        {
            try
            {
                await _tableDatatableModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // El circuito ya está desconectado, no podemos hacer llamadas de JavaScript interop
                // Esto es normal cuando el componente se está eliminando
            }
        }

        if (_formWizardModule is not null)
        {
            try
            {
                await _formWizardModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // El circuito ya está desconectado, no podemos hacer llamadas de JavaScript interop
                // Esto es normal cuando el componente se está eliminando
            }
        }

        // Dispose DotNetObjectReference
        if (_dotNetObjectReference is not null)
        {
            try
            {
                _dotNetObjectReference.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        GC.SuppressFinalize(this);
    }
}
