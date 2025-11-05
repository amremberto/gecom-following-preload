using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.DocumentTypes;

/// <summary>
/// Represents a document type entity in the preload system.
/// </summary>
/// <remarks>
/// This entity defines the different types of documents that can be processed,
/// including their codes, descriptions, and whether they are FEC (electronic fiscal) documents.
/// </remarks>
public partial class DocumentType : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique document type identifier.
    /// </summary>
    public int TipoDocId { get; set; }

    /// <summary>
    /// Gets or sets the description of this document type.
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the letter code for this document type.
    /// </summary>
    public string? Letra { get; set; }

    /// <summary>
    /// Gets or sets the code that identifies this document type.
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the extended description of this document type.
    /// </summary>
    public string? DescripcionLarga { get; set; }

    /// <summary>
    /// Gets or sets the creation date of this document type.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this document type.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document type is FEC (electronic fiscal).
    /// </summary>
    public bool IsFec { get; set; }

    /// <summary>
    /// Gets or sets the collection of documents that use this document type.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = [];
}
