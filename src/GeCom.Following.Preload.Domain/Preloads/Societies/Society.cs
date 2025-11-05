using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.Societies;

/// <summary>
/// Represents a society entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about societies including their codes,
/// descriptions, tax identification numbers, and preload status.
/// </remarks>
public partial class Society : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique society identifier.
    /// </summary>
    public int SocId { get; set; }

    /// <summary>
    /// Gets or sets the society code.
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the society description.
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the society's CUIT (tax identification number).
    /// </summary>
    public string Cuit { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation date of this society.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this society.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this society is for preload.
    /// </summary>
    public bool? EsPrecarga { get; set; }

    /// <summary>
    /// Gets or sets the collection of documents associated with this society.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
