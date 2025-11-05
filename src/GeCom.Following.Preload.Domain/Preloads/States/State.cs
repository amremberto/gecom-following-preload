using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.States;

/// <summary>
/// Represents a state entity in the preload system.
/// </summary>
/// <remarks>
/// This entity defines the different states that documents and other entities 
/// can have within the system, including creation and deletion tracking.
/// </remarks>
public partial class State : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique state identifier.
    /// </summary>
    public int EstadoId { get; set; }

    /// <summary>
    /// Gets or sets the description of this state.
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the code that identifies this state.
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation date of this state.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this state.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets the collection of documents associated with this state.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    /// <summary>
    /// Gets or sets the collection of document states associated with this state.
    /// </summary>
    public virtual ICollection<DocumentState> DocumentStates { get; set; } = new List<DocumentState>();
}
