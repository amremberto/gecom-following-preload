using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.DocumentStates;

/// <summary>
/// Represents a document state entity in the preload system.
/// </summary>
/// <remarks>
/// This entity represents the relationship between documents and their states,
/// tracking state changes and timestamps for audit purposes.
/// </remarks>
public partial class DocumentState : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique document state identifier.
    /// </summary>
    public int EstDocId { get; set; }

    /// <summary>
    /// Gets or sets the state identifier.
    /// </summary>
    public int EstadoId { get; set; }

    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Gets or sets the creation date of this document state.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this document state.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets the document associated with this state.
    /// </summary>
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the state associated with this document.
    /// </summary>
    public virtual State State { get; set; } = null!;
}
