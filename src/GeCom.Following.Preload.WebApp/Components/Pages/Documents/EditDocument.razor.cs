using System.Globalization;
using System.Security.Claims;
using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Documents.Update;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.WebApp.Components.Shared;
using GeCom.Following.Preload.WebApp.Enums;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using GeCom.Following.Preload.WebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Pages.Documents;

/// <summary>
/// Component for editing documents.
/// </summary>
public partial class EditDocument : IAsyncDisposable
{
    [Parameter] public int DocId { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IDocumentService DocumentService { get; set; } = default!;
    [Inject] private ICurrencyService CurrencyService { get; set; } = default!;
    [Inject] private IDocumentTypeService DocumentTypeService { get; set; } = default!;
    [Inject] private ISapProviderSocietyService SapProviderSocietyService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private bool _isLoading = true;
    private IJSObjectReference? _formWizardModule;
    private DotNetObjectReference<EditDocument>? _dotNetObjectReference;

    private bool _isProvider;
    private string? _providerCuit;
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
        // In Blazor Server with InteractiveServer, OnInitializedAsync is called twice:
        // 1. During server-side pre-rendering
        // 2. When the SignalR connection is established
        // To avoid duplicate API calls, we only initialize state here and load data in OnAfterRenderAsync
        _isLoading = true;
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
                await CheckIfProviderAndGetCuitAsync();

                StateHasChanged();

                _formWizardModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/components/form-wizard.min.js");
                await InvokeAsync(StateHasChanged);

                // Load theme config and apps before initializing form components
                await JsRuntime.InvokeVoidAsync("loadThemeConfig");
                await JsRuntime.InvokeVoidAsync("loadApps");

                // Load document and initialize form
                await LoadDocumentAndInitializeForm();

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _isLoading = false;
            await JsRuntime.InvokeVoidAsync("console.error", "Error en EditDocument.OnAfterRenderAsync:", ex.Message);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Loads the document and initializes the form with all necessary data.
    /// </summary>
    private async Task LoadDocumentAndInitializeForm()
    {
        try
        {
            _isLoading = true;
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

            await GetDocumentWithDetails(DocId);
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

            _isLoading = false;
            StateHasChanged();

            if (_selectedDocument is not null)
            {
                // Initialize SELECT2 and Flatpickr after DOM is updated (same timing as Pending.razor)
                await Task.Delay(300); // Delay to ensure DOM is fully rendered

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

                // Initialize data tables for purchase orders and notes
                if (_selectedDocument.PurchaseOrders.Any())
                {
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "edit-document-oc-datatable");
                }
                if (_selectedDocument.Notes.Any())
                {
                    await JsRuntime.InvokeVoidAsync("loadDataTable", "edit-document-notes-datatable");
                }
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar el documento para editar: respuesta nula");
                await ShowToast("Error al cargar el documento para editar.");
            }
        }
        catch (Exception ex)
        {
            _isLoading = false;
            await JsRuntime.InvokeVoidAsync("console.error", "Error al cargar documento:", ex.Message);
            await ShowToast("Error al cargar el documento.");
            StateHasChanged();
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
            await JsRuntime.InvokeVoidAsync("initDocumentTypeSelect2", _selectedDocumentTypeId?.ToString(CultureInfo.InvariantCulture));
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
    /// Updates the document from JavaScript when user clicks "Siguiente" with changes.
    /// </summary>
    /// <param name="docId">The document ID.</param>
    /// <param name="sociedadCuit">The sociedad CUIT.</param>
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

            // Build update request
            UpdateDocumentRequest request = new(
                DocId: docId,
                ProveedorCuit: _selectedDocument.ProveedorCuit,
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
            _selectedPuntoDeVenta = updatedDocument.PuntoDeVenta ?? string.Empty;
            _selectedNumeroComprobante = updatedDocument.NumeroComprobante ?? string.Empty;
            _selectedCaecai = updatedDocument.Caecai ?? string.Empty;
            _selectedMontoBruto = updatedDocument.MontoBruto;

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

            // Dispose previous reference if exists
            _dotNetObjectReference?.Dispose();

            _dotNetObjectReference = DotNetObjectReference.Create(this);
            int docId = _selectedDocument?.DocId ?? 0;
            await JsRuntime.InvokeVoidAsync("initEditDocumentWizard", isPendingPreload, _dotNetObjectReference, docId);
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("console.error", "Error al inicializar el wizard:", ex.Message);
        }
    }

    /// <summary>
    /// Handles the PDF file selection.
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
    /// Shows a toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The toast type.</param>
    private async Task ShowToast(string message, ToastType type = ToastType.Error)
    {
        await ToastService.ShowAsync(message, type);
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
    /// Navigates back to the pending documents page.
    /// </summary>
    private void NavigateToDocuments()
    {
        NavigationManager.NavigateTo("/documents/pending");
    }

    public async ValueTask DisposeAsync()
    {
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
