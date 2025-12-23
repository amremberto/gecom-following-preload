using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;

/// <summary>
/// Repository interface for Attachment entities.
/// </summary>
public interface IAttachmentRepository : IRepository<Attachment>
{
    /// <summary>
    /// Gets attachments by document ID.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of attachments.</returns>
    Task<IEnumerable<Attachment>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attachments by file path.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of attachments.</returns>
    Task<IEnumerable<Attachment>> GetByPathAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attachments created within a date range.
    /// </summary>
    /// <param name="startDate">Start date.</param>
    /// <param name="endDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of attachments.</returns>
    Task<IEnumerable<Attachment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active attachment (FechaBorrado == null) for a document with tracking enabled.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active attachment if found, otherwise null.</returns>
    Task<Attachment?> GetActiveAttachmentByDocumentIdAsync(int docId, CancellationToken cancellationToken = default);
}
