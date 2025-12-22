using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Document entities.
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    /// <summary>
    /// Gets documents by provider CUIT.
    /// </summary>
    /// <param name="proveedorCuit">Provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByProveedorCuitAsync(string proveedorCuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by society CUIT.
    /// </summary>
    /// <param name="sociedadCuit">Society CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetBySociedadCuitAsync(string sociedadCuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by document type ID.
    /// </summary>
    /// <param name="tipoDocId">Document type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByTipoDocIdAsync(int tipoDocId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by state ID.
    /// </summary>
    /// <param name="estadoId">State ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByEstadoIdAsync(int estadoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by currency code.
    /// </summary>
    /// <param name="moneda">Currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByMonedaAsync(string moneda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by document number.
    /// </summary>
    /// <param name="numeroComprobante">Document number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByNumeroComprobanteAsync(string numeroComprobante, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by barcode.
    /// </summary>
    /// <param name="codigoDeBarras">Barcode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByCodigoDeBarrasAsync(string codigoDeBarras, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by CAECAI.
    /// </summary>
    /// <param name="caecai">CAECAI code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByCaecaiAsync(string caecai, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by user who created them.
    /// </summary>
    /// <param name="userCreate">User who created the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByUserCreateAsync(string userCreate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents created within a date range.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by amount range.
    /// </summary>
    /// <param name="minAmount">Minimum amount.</param>
    /// <param name="maxAmount">Maximum amount.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range and optionally by provider CUIT.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="providerCuit">Provider CUIT. If null, returns documents from all providers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByEmissionDatesAndProviderCuitAsync(DateOnly dateFrom, DateOnly dateTo, string? providerCuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range and multiple society CUITs.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="societyCuits">Collection of society CUITs. If empty, returns documents from all societies.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByEmissionDatesAndSocietyCuitsAsync(DateOnly dateFrom, DateOnly dateTo, IEnumerable<string> societyCuits, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending documents by provider CUIT.
    /// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="providerCuit">Provider CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of pending documents.</returns>
    Task<IEnumerable<Document>> GetPendingByProviderCuitAsync(string providerCuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending documents.
    /// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of pending documents.</returns>
    Task<IEnumerable<Document>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending documents by multiple society CUITs.
    /// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
    /// </summary>
    /// <param name="societyCuits">Collection of society CUITs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of pending documents.</returns>
    Task<IEnumerable<Document>> GetPendingBySocietyCuitsAsync(IEnumerable<string> societyCuits, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range and state ID, optionally filtered by provider CUIT.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="estadoId">State ID to filter by.</param>
    /// <param name="providerCuit">Provider CUIT. If null, returns documents from all providers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByEmissionDatesAndEstadoIdAsync(DateOnly dateFrom, DateOnly dateTo, int estadoId, string? providerCuit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents by emission date range, state ID, and multiple society CUITs.
    /// </summary>
    /// <param name="dateFrom">Start emission date.</param>
    /// <param name="dateTo">End emission date.</param>
    /// <param name="estadoId">State ID to filter by.</param>
    /// <param name="societyCuits">Collection of society CUITs. If empty, returns documents from all societies.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of documents.</returns>
    Task<IEnumerable<Document>> GetByEmissionDatesEstadoIdAndSocietyCuitsAsync(DateOnly dateFrom, DateOnly dateTo, int estadoId, IEnumerable<string> societyCuits, CancellationToken cancellationToken = default);
}
