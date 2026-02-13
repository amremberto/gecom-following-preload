namespace GeCom.Following.Preload.Application.Abstractions.Pdf;

/// <summary>
/// Service for generating PDF documents.
/// </summary>
public interface IPdfDocumentService
{
    /// <summary>
    /// Generates a PDF document and returns its content as a byte array.
    /// </summary>
    /// <param name="document">Descriptor of the document to generate (type, data).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PDF file content.</returns>
    Task<byte[]> GenerateAsync(PdfDocumentRequest document, CancellationToken cancellationToken = default);
}
