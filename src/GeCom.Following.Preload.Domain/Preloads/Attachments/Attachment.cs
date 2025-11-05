using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.Attachments;

/// <summary>
/// Represents an attachment entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about file attachments associated with documents,
/// including file paths and creation/modification timestamps.
/// </remarks>
public partial class Attachment : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique attachment identifier.
    /// </summary>
    public int AdjuntoId { get; set; }

    /// <summary>
    /// Gets or sets the file path of this attachment.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation date of this attachment.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the last modification date of this attachment.
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this attachment.
    /// </summary>
    public DateTime? FechaBorrado { get; set; }

    /// <summary>
    /// Gets or sets the document identifier associated with this attachment.
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Gets or sets the document associated with this attachment.
    /// </summary>
    public virtual Document Doc { get; set; } = null!;
}
