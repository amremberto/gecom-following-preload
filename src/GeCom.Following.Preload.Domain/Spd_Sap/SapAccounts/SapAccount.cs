using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;

/// <summary>
/// Represents a SAP account entity in the system.
/// </summary>
/// <remarks>
/// This entity contains information about accounts from the SAP system,
/// including contact details, addresses, and business information.
/// </remarks>
public partial class SapAccount : BaseEntity
{
    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string Accountnumber { get; set; } = null!;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the city of the address.
    /// </summary>
    public string? Address1City { get; set; }

    /// <summary>
    /// Gets or sets the state or province of the address.
    /// </summary>
    public string? Address1Stateorprovince { get; set; }

    /// <summary>
    /// Gets or sets the postal code of the address.
    /// </summary>
    public string? Address1Postalcode { get; set; }

    /// <summary>
    /// Gets or sets the first line of the address.
    /// </summary>
    public string? Address1Line1 { get; set; }

    /// <summary>
    /// Gets or sets the primary telephone number.
    /// </summary>
    public string? Telephone1 { get; set; }

    /// <summary>
    /// Gets or sets the fax number.
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Gets or sets the country of the address.
    /// </summary>
    public string? Address1Country { get; set; }

    /// <summary>
    /// Gets or sets the CUIT (tax identification number).
    /// </summary>
    public string? NewCuit { get; set; }

    /// <summary>
    /// Gets or sets the blocked status.
    /// </summary>
    public string? NewBloqueado { get; set; }

    /// <summary>
    /// Gets or sets the business sector.
    /// </summary>
    public string? NewRubro { get; set; }

    /// <summary>
    /// Gets or sets the IIBB (provincial tax) information.
    /// </summary>
    public string? NewIibb { get; set; }

    /// <summary>
    /// Gets or sets the primary email address.
    /// </summary>
    public string? Emailaddress1 { get; set; }

    /// <summary>
    /// Gets or sets the customer type code.
    /// </summary>
    public int? Customertypecode { get; set; }

    /// <summary>
    /// Gets or sets the provider group.
    /// </summary>
    public string? NewGproveedor { get; set; }

    /// <summary>
    /// Gets or sets the CBU (bank account number).
    /// </summary>
    public string? Cbu { get; set; }
}
