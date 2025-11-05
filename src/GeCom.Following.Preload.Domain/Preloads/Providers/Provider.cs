using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.Providers;

/// <summary>
/// Represents a provider entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about providers including their tax identification,
/// business name, contact information, and associated documents.
/// </remarks>
public partial class Provider : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique provider identifier.
    /// </summary>
    public int ProvId { get; set; }

    /// <summary>
    /// Gets or sets the provider's CUIT (tax identification number).
    /// </summary>
    public string Cuit { get; set; } = null!;

    /// <summary>
    /// Gets or sets the provider's business name.
    /// </summary>
    public string RazonSocial { get; set; } = null!;

    /// <summary>
    /// Gets or sets the provider's email address.
    /// </summary>
    public string Mail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation date of this provider.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this provider.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets the collection of documents associated with this provider.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = [];
}
