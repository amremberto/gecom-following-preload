using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Documents.Update;
using Microsoft.AspNetCore.Components.Forms;

namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Service for document-related operations.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Gets all documents from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    Task<IEnumerable<DocumentResponse>?> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range from the API.
    /// The filtering is automatically handled by the backend based on user role:
    /// - Providers: Filters by provider CUIT from claim
    /// - Societies: Filters by all societies assigned to the user
    /// - Administrator/ReadOnly: Returns all documents without filtering
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents matching the criteria based on user role.</returns>
    Task<IEnumerable<DocumentResponse>?> GetByDatesAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paid documents by emission date range from the API.
    /// Paid documents are those with state code "PagadoFin".
    /// The filtering is automatically handled by the backend based on user role:
    /// - Providers: Filters by provider CUIT from claim
    /// - Societies: Filters by all societies assigned to the user
    /// - Administrator/ReadOnly: Returns all paid documents without filtering
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of paid documents matching the criteria based on user role.</returns>
    Task<IEnumerable<DocumentResponse>?> GetPaidDocumentsByDatesAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range and provider CUIT from the API.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="providerCuit">Provider CUIT (required). Must match the CUIT in the user's claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents matching the criteria.</returns>
    Task<IEnumerable<DocumentResponse>?> GetByDatesAndProviderAsync(
        DateOnly dateFrom,
        DateOnly dateTo,
        string providerCuit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a document by its ID from the API.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document if found.</returns>
    Task<DocumentResponse?> GetByIdAsync(
        int docId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Preloads a document by uploading a PDF file.
    /// </summary>
    /// <param name="file">The PDF file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created document with attachment.</returns>
    Task<DocumentResponse?> PreloadDocumentAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a PDF file by attachment ID.
    /// </summary>
    /// <param name="adjuntoId">Attachment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PDF file content as byte array.</returns>
    Task<byte[]?> DownloadAttachmentAsync(
        int adjuntoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending documents by provider CUIT from the API.
    /// Pending documents are those with EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="providerCuit">Provider CUIT (required). Must match the CUIT in the user's claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of pending documents for the specified provider.</returns>
    Task<IEnumerable<DocumentResponse>?> GetPendingDocumentsByProviderAsync(
        string providerCuit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending documents based on user role from the API.
    /// The filtering is automatically handled by the backend based on user role:
    /// - ReadOnly/Administrator: Returns all pending documents.
    /// - Societies: Returns pending documents for all societies assigned to the user.
    /// Note: Providers should use GetPendingDocumentsByProviderAsync instead.
    /// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of pending documents based on user role.</returns>
    Task<IEnumerable<DocumentResponse>?> GetPendingDocumentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing document.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="request">The document data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document.</returns>
    Task<DocumentResponse?> UpdateAsync(
        int docId,
        UpdateDocumentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the PDF file associated with an existing document.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="file">The new PDF file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document with the new PDF attachment.</returns>
    Task<DocumentResponse?> UpdateDocumentPdfAsync(
        int docId,
        IBrowserFile file,
        CancellationToken cancellationToken = default);
}

