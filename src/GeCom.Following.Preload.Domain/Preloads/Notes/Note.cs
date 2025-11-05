using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.Notes;

/// <summary>
/// Represents a note entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about notes associated with documents,
/// including descriptions and creation tracking.
/// </remarks>
public partial class Note : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique note identifier.
    /// </summary>
    public int NotaId { get; set; }

    /// <summary>
    /// Gets or sets the description of this note.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Gets or sets the user who created this note.
    /// </summary>
    public string UsuarioCreacion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation date of this note.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the document identifier associated with this note.
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Gets or sets the document associated with this note.
    /// </summary>
    public virtual Document Document { get; set; } = null!;
}
