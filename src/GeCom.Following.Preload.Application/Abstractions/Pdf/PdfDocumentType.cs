namespace GeCom.Following.Preload.Application.Abstractions.Pdf;

/// <summary>
/// Supported PDF document types for generation.
/// </summary>
public enum PdfDocumentType
{
    /// <summary>
    /// Simple placeholder/sample document (title + content).
    /// </summary>
    Placeholder = 0,

    /// <summary>
    /// Payment confirmation / comprobante de pago (for future use).
    /// </summary>
    ComprobantePago = 1,

    /// <summary>
    /// Recibo de Pago (payment receipt).
    /// </summary>
    ReciboDePago = 2
}
