namespace GeCom.Following.Preload.Application.Abstractions.Pdf;

/// <summary>
/// Data required to generate a Recibo de Pago (payment receipt) PDF.
/// </summary>
public sealed class ReciboDePagoData
{
    /// <summary>
    /// Receipt number (e.g. "00046").
    /// </summary>
    public string NumeroRecibo { get; set; } = string.Empty;

    /// <summary>
    /// Emission date of the receipt.
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Provider CUIT.
    /// </summary>
    public string ProveedorCuit { get; set; } = string.Empty;

    /// <summary>
    /// Provider business name.
    /// </summary>
    public string ProveedorRazonSocial { get; set; } = string.Empty;

    /// <summary>
    /// Optional provider number or identifier.
    /// </summary>
    public string? ProveedorNro { get; set; }

    /// <summary>
    /// Optional provider address (e.g. "Calle Nro, Localidad").
    /// </summary>
    public string? ProveedorDireccion { get; set; }

    /// <summary>
    /// Optional provider phone.
    /// </summary>
    public string? ProveedorTelefono { get; set; }

    /// <summary>
    /// Optional order of payment number.
    /// </summary>
    public string? OrdenDePago { get; set; }

    /// <summary>
    /// Client/payer name (e.g. "Exponenciar S. A.").
    /// </summary>
    public string Cliente { get; set; } = string.Empty;

    /// <summary>
    /// Payment concept (e.g. "Pago a Proveedores.").
    /// </summary>
    public string Concepto { get; set; } = string.Empty;

    /// <summary>
    /// Received amount.
    /// </summary>
    public decimal ImporteRecibido { get; set; }

    /// <summary>
    /// Optional currency code.
    /// </summary>
    public string? Moneda { get; set; }

    /// <summary>
    /// Registration date of the payment detail.
    /// </summary>
    public DateTime FechaAlta { get; set; }

    /// <summary>
    /// True when payment method is transfer; false for cheque/echeq.
    /// </summary>
    public bool EsTransferencia { get; set; }

    /// <summary>
    /// Check number (when EsTransferencia is false).
    /// </summary>
    public string? NroCheque { get; set; }

    /// <summary>
    /// Bank name (when EsTransferencia is false).
    /// </summary>
    public string? Banco { get; set; }

    /// <summary>
    /// Cheque due date (when EsTransferencia is false).
    /// </summary>
    public DateOnly? Vencimiento { get; set; }
}
