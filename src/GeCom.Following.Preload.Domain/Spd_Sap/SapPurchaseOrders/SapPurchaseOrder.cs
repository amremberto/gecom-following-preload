using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Represents a SAP purchase order entity in the system.
/// </summary>
/// <remarks>
/// This entity contains information about purchase orders from the SAP system,
/// including quantities, materials, suppliers, and financial details.
/// </remarks>
public partial class SapPurchaseOrder : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique order identifier.
    /// </summary>
    public long Idorden { get; set; }

    /// <summary>
    /// Gets or sets the document date.
    /// </summary>
    public DateTime? Fechadocumento { get; set; }

    /// <summary>
    /// Gets or sets the document number.
    /// </summary>
    public string Nrodocumento { get; set; } = null!;

    /// <summary>
    /// Gets or sets the position within the order.
    /// </summary>
    public int Posicion { get; set; }

    /// <summary>
    /// Gets or sets the material code.
    /// </summary>
    public string? Material { get; set; }

    /// <summary>
    /// Gets or sets the brief description.
    /// </summary>
    public string? Textobreve { get; set; }

    /// <summary>
    /// Gets or sets the financial society code.
    /// </summary>
    public string? Codigosociedadfi { get; set; }

    /// <summary>
    /// Gets or sets the company code.
    /// </summary>
    public string? Empresa { get; set; }

    /// <summary>
    /// Gets or sets the center code.
    /// </summary>
    public string? Centro { get; set; }

    /// <summary>
    /// Gets or sets the warehouse code.
    /// </summary>
    public string? Almacen { get; set; }

    /// <summary>
    /// Gets or sets the requested quantity.
    /// </summary>
    public decimal Cantidadpedida { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure for requested quantity.
    /// </summary>
    public string? UnidadCp { get; set; }

    /// <summary>
    /// Gets or sets the delivered quantity.
    /// </summary>
    public decimal Cantidadentregada { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure for delivered quantity.
    /// </summary>
    public string? UnidadCe { get; set; }

    /// <summary>
    /// Gets or sets the invoiced quantity.
    /// </summary>
    public decimal Cantidadfacturada { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure for invoiced quantity.
    /// </summary>
    public string? UnidadCf { get; set; }

    /// <summary>
    /// Gets or sets the supplier code.
    /// </summary>
    public string Proveedor { get; set; } = null!;

    /// <summary>
    /// Gets or sets the payment condition.
    /// </summary>
    public string? Condicionpago { get; set; }

    /// <summary>
    /// Gets or sets the user who created the order.
    /// </summary>
    public string? Usuario { get; set; }

    /// <summary>
    /// Gets or sets the original amount.
    /// </summary>
    public decimal Importeoriginal { get; set; }

    /// <summary>
    /// Gets or sets the delivery address.
    /// </summary>
    public string? Direccionentrega { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the order is deleted.
    /// </summary>
    public bool? Borrado { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the order is blocked.
    /// </summary>
    public bool? Bloqueado { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the final delivery.
    /// </summary>
    public bool? EntregaFinal { get; set; }

    /// <summary>
    /// Gets or sets the order type.
    /// </summary>
    public string? Tipo { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string? Moneda { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string? Localidad { get; set; }

    /// <summary>
    /// Gets or sets the release status.
    /// </summary>
    public int Liberada { get; set; }

    /// <summary>
    /// Gets or sets the distribution code.
    /// </summary>
    public string? Dist { get; set; }

    /// <summary>
    /// Gets or sets the advance net amount.
    /// </summary>
    public decimal? NetoAnticipo { get; set; }
}
