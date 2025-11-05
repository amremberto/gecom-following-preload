using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.Currencies;

/// <summary>
/// Represents a currency entity in the preload system.
/// </summary>
/// <remarks>
/// This entity defines the different currencies that can be used in documents,
/// including their codes, descriptions, and AFIP (tax authority) codes.
/// </remarks>
public partial class Currency : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique currency identifier.
    /// </summary>
    public int MonedaId { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the currency description.
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the AFIP (tax authority) currency code.
    /// </summary>
    public string? CodigoAfip { get; set; }

    /// <summary>
    /// Gets or sets the collection of documents that use this currency.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
