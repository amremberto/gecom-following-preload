
using System.Globalization;
using System.Security.Claims;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
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
    private string? _providerCuit;
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
    private IEnumerable<ProviderSocietyResponse> _providerSocieties = [];
    private string? _selectedSocietyCuit;
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

                // Open the edit modal for the document
                await OpenDocumentEdit(docId);
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
    private static bool IsPendingPreloadStatus(DocumentResponse document)
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

        await GetDocumentWithDetails(docId);
        await LoadCurrencies();
        await LoadDocumentTypes();

        // Load provider societies if user is a provider
        if (_isProvider && !string.IsNullOrWhiteSpace(_providerCuit))
        {
            await LoadProviderSocieties();
        }

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
            // Set the selected text fields from the document
            _selectedPuntoDeVenta = _selectedDocument.PuntoDeVenta ?? string.Empty;
            _selectedNumeroComprobante = _selectedDocument.NumeroComprobante ?? string.Empty;
            _selectedCaecai = _selectedDocument.Caecai ?? string.Empty;
            _selectedMontoBruto = _selectedDocument.MontoBruto;
        }

        _isModalLoading = false;
        StateHasChanged();

        if (_selectedDocument is not null)
        {
            // Show edit modal and set up cleanup on close
            await JsRuntime.InvokeVoidAsync("eval", @"
                (function() {
                    var modalElement = document.getElementById('edit-document-modal');
                    var modal = new bootstrap.Modal(modalElement);
                    
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
                    }, { once: true });
                    
                    modal.show();
                })();
            ");

            // Initialize SELECT2 and Flatpickr after modal is shown and DOM is updated
            await Task.Delay(300); // Delay to ensure modal and DOM are fully rendered

            // Force StateHasChanged to ensure DOM is updated with current values before initializing SELECT2
            StateHasChanged();
            await Task.Delay(100); // Small delay to ensure DOM update is complete

            await InitializeCurrencySelect2();
            await InitializeDocumentTypeSelect2();

            // Initialize society SELECT2 only if user is a provider
            if (_isProvider && !string.IsNullOrWhiteSpace(_providerCuit))
            {
                await InitializeSocietySelect2();
            }

            await InitializeDatePickers();

            // Initialize wizard with validation
            await InitializeEditDocumentWizard();
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
    /// Loads all provider societies from the API.
    /// </summary>
    /// <returns></returns>
    private async Task LoadProviderSocieties()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_providerCuit))
            {
                _providerSocieties = [];
                return;
            }

            IEnumerable<ProviderSocietyResponse>? response = await SapProviderSocietyService.GetSocietiesByProviderCuitAsync(
                _providerCuit,
                cancellationToken: default);
            _providerSocieties = response ?? [];
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar sociedades del proveedor:", ex.Message);
            await ShowToast("Error al cargar las sociedades disponibles.");
            _providerSocieties = [];
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
            await JsRuntime.InvokeVoidAsync("initSocietySelect2", _selectedSocietyCuit);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar SELECT2 para cliente:", ex.Message);
        }
    }

    /// <summary>
    /// Handles the society selection change.
    /// </summary>
    /// <param name="e">The change event arguments.</param>
    /// <returns></returns>
    private Task OnSocietyChanged(ChangeEventArgs e)
    {
        _selectedSocietyCuit = e.Value?.ToString();
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
            // Initialize Fecha Factura
            if (_selectedFechaFactura.HasValue)
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#fecha-factura-edit",
                    new { defaultDate = _selectedFechaFactura.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#fecha-factura-edit",
                    new { });
            }

            // Initialize Venc. CAE / CAI
            if (_selectedVencimientoCaecai.HasValue)
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#venc-caecai-edit",
                    new { defaultDate = _selectedVencimientoCaecai.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("initFlatpickrWithStrictValidation",
                    "#venc-caecai-edit",
                    new { });
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
    /// Initializes the edit document wizard with validation.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeEditDocumentWizard()
    {
        try
        {
            bool isPendingPreload = _selectedDocument is not null && IsPendingPreloadStatus(_selectedDocument);

            // Dispose previous reference if exists
            _dotNetObjectReference?.Dispose();

            _dotNetObjectReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("initEditDocumentWizard", isPendingPreload, _dotNetObjectReference);
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
            await OpenDocumentEdit(document.DocId);
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
