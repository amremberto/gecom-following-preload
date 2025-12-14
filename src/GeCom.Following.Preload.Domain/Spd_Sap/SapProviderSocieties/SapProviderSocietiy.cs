using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Spd_Sap.SapProviderSocieties;

public partial class SapProviderSocietiy : BaseEntity
{
    /// <summary>
    /// Gets or sets the provider identifier. (AccountNumber)
    /// </summary>
    public string? Proveedor { get; set; }
    /// <summary>
    /// Gets or sets the financial society identifier.
    /// </summary>
    public string? Sociedadfi { get; set; }
    /// <summary>
    /// Gets or sets the payment terms.
    /// </summary>
    public string? CondicionPago { get; set; }
    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public string? Viapago { get; set; }
    /// <summary>
    /// Gets or sets the blocked status.
    /// </summary>
    public string? Bloqueado { get; set; }
    /// <summary>
    /// Gets or sets the CBU (bank account identifier).
    /// </summary>
    public string? CBU { get; set; }
    /// <summary>
    /// Gets or sets the code associated with the entity.
    /// </summary>
    public string? Codigo { get; set; }
}
