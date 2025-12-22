using GeCom.Following.Preload.Contracts.Preload.Documents;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GeCom.Following.Preload.WebApp.Components.Modals;

public partial class ConfirmPaymentModal : ComponentBase
{
    private bool _isProcessing;
    private string _errorMessage = string.Empty;
    private string _selectedPaymentMethod = "Transferencia";
    private string _numeroCheque = string.Empty;
    private string _banco = string.Empty;
    private DateOnly? _vencimiento;
    private DocumentResponse? _selectedDocument;
    private readonly Dictionary<string, string> _validationErrors = new();
    private readonly object _paymentModel = new();

    [Parameter] public EventCallback<PaymentConfirmationData> OnPaymentConfirmed { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// Determines if the confirm payment button should be enabled.
    /// </summary>
    private bool CanConfirmPayment
    {
        get
        {
            if (_selectedPaymentMethod == "Transferencia")
            {
                return true;
            }

            // For "Cheque o echeq", all fields must be filled
            return !string.IsNullOrWhiteSpace(_numeroCheque) &&
                   !string.IsNullOrWhiteSpace(_banco) &&
                   _vencimiento.HasValue &&
                   _validationErrors.Count == 0;
        }
    }

    /// <summary>
    /// Gets the CSS class for the numero cheque input field.
    /// </summary>
    private string NumeroChequeClass => _validationErrors.ContainsKey("NumeroCheque") ? "form-control is-invalid" : "form-control";

    /// <summary>
    /// Gets the CSS class for the banco input field.
    /// </summary>
    private string BancoClass => _validationErrors.ContainsKey("Banco") ? "form-control is-invalid" : "form-control";

    /// <summary>
    /// Gets the CSS class for the vencimiento input field.
    /// </summary>
    private string VencimientoClass => _validationErrors.ContainsKey("Vencimiento") ? "form-control is-invalid" : "form-control";

    /// <summary>
    /// Handles the payment method selection change.
    /// </summary>
    /// <param name="method">The selected payment method.</param>
    private void OnPaymentMethodChanged(string method)
    {
        _selectedPaymentMethod = method;
        _validationErrors.Clear();
        _errorMessage = string.Empty;
        
        // Reset cheque fields when switching to Transferencia
        if (method == "Transferencia")
        {
            _numeroCheque = string.Empty;
            _banco = string.Empty;
            _vencimiento = null;
        }
        
        StateHasChanged();
    }

    /// <summary>
    /// Validates the cheque fields.
    /// </summary>
    private void ValidateChequeFields()
    {
        _validationErrors.Clear();

        if (string.IsNullOrWhiteSpace(_numeroCheque))
        {
            _validationErrors["NumeroCheque"] = "El número de cheque es requerido.";
        }

        if (string.IsNullOrWhiteSpace(_banco))
        {
            _validationErrors["Banco"] = "El banco es requerido.";
        }

        if (!_vencimiento.HasValue)
        {
            _validationErrors["Vencimiento"] = "La fecha de vencimiento es requerida.";
        }

        StateHasChanged();
    }

    /// <summary>
    /// Handles the confirm payment action.
    /// </summary>
    private async Task HandleConfirmPayment()
    {
        if (_selectedDocument is null)
        {
            _errorMessage = "No se ha seleccionado un documento.";
            return;
        }

        // Validate cheque fields if payment method is cheque
        if (_selectedPaymentMethod == "Cheque o echeq")
        {
            ValidateChequeFields();
            if (_validationErrors.Count > 0)
            {
                _errorMessage = "Por favor complete todos los campos requeridos.";
                return;
            }
        }

        try
        {
            _isProcessing = true;
            _errorMessage = string.Empty;
            StateHasChanged();

            // Create payment confirmation data
            var paymentData = new PaymentConfirmationData
            {
                DocumentId = _selectedDocument.DocId,
                PaymentMethod = _selectedPaymentMethod,
                NumeroCheque = _selectedPaymentMethod == "Cheque o echeq" ? _numeroCheque : null,
                Banco = _selectedPaymentMethod == "Cheque o echeq" ? _banco : null,
                Vencimiento = _selectedPaymentMethod == "Cheque o echeq" ? _vencimiento : null
            };

            // Close modal
            await JsRuntime.InvokeVoidAsync("eval", "bootstrap.Modal.getInstance(document.getElementById('confirmPaymentModal'))?.hide()");

            // Notify parent component
            await OnPaymentConfirmed.InvokeAsync(paymentData);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error al confirmar el pago: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Closes the modal and resets the form.
    /// </summary>
    private async Task CloseModal()
    {
        await OnCancel.InvokeAsync();
        ResetForm();
    }

    /// <summary>
    /// Resets the form to its initial state.
    /// </summary>
    private void ResetForm()
    {
        _selectedDocument = null;
        _selectedPaymentMethod = "Transferencia";
        _numeroCheque = string.Empty;
        _banco = string.Empty;
        _vencimiento = null;
        _errorMessage = string.Empty;
        _validationErrors.Clear();
        _isProcessing = false;
    }

    /// <summary>
    /// Shows the modal with the specified document.
    /// </summary>
    /// <param name="document">The document to confirm payment for.</param>
    public async Task ShowAsync(DocumentResponse document)
    {
        ResetForm();
        _selectedDocument = document;
        StateHasChanged();
        await JsRuntime.InvokeVoidAsync("eval", "new bootstrap.Modal(document.getElementById('confirmPaymentModal')).show()");
    }
}

/// <summary>
/// Data class for payment confirmation.
/// </summary>
public class PaymentConfirmationData
{
    public int DocumentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? NumeroCheque { get; set; }
    public string? Banco { get; set; }
    public DateOnly? Vencimiento { get; set; }
}
