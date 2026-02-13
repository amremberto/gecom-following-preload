namespace GeCom.Following.Preload.Application.Abstractions.Pdf;

/// <summary>
/// Descriptor for a PDF document to generate.
/// </summary>
public sealed class PdfDocumentRequest
{
    /// <summary>
    /// Type of document to generate.
    /// </summary>
    public PdfDocumentType DocumentType { get; set; }

    /// <summary>
    /// Title of the document (used for Placeholder and similar types).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Body or content text (used for Placeholder and similar types).
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Optional additional data for document-type-specific content (e.g. comprobante, report).
    /// </summary>
    public IReadOnlyDictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Data for Recibo de Pago when DocumentType is ReciboDePago.
    /// </summary>
    public ReciboDePagoData? ReciboDePagoData { get; set; }
}
