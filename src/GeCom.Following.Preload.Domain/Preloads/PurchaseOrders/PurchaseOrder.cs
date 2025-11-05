using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;

/// <summary>
/// Represents a purchase order entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about purchase orders including quantities, 
/// codes, and references to related documents and SAP systems.
/// </remarks>
public partial class PurchaseOrder : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique purchase order identifier.
    /// </summary>
    public int Ocid { get; set; }

    /// <summary>
    /// Gets or sets the reception code for this purchase order.
    /// </summary>
    public string? CodigoRecepcion { get; set; }

    /// <summary>
    /// Gets or sets the quantity to be invoiced.
    /// </summary>
    public decimal CantidadAfacturar { get; set; }

    /// <summary>
    /// Gets or sets the document identifier associated with this purchase order.
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Gets or sets the purchase order identifier.
    /// </summary>
    public int OrdenCompraId { get; set; }

    /// <summary>
    /// Gets or sets the creation date of this purchase order.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the deletion date of this purchase order.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets the purchase order number.
    /// </summary>
    public string NroOc { get; set; } = null!;

    /// <summary>
    /// Gets or sets the position within the purchase order.
    /// </summary>
    public int PosicionOc { get; set; }

    /// <summary>
    /// Gets or sets the financial society code.
    /// </summary>
    public string CodigoSociedadFi { get; set; } = null!;

    /// <summary>
    /// Gets or sets the SAP provider identifier.
    /// </summary>
    public string ProveedorSap { get; set; } = null!;

    /// <summary>
    /// Gets or sets the document associated with this purchase order.
    /// </summary>
    public virtual Document Document { get; set; } = null!;
}
