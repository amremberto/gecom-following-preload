namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts.Create;

/// <summary>
/// Request DTO for creating a SAP account.
/// </summary>
public sealed record CreateSapAccountRequest(
    string Accountnumber,
    string? Name = null,
    string? Address1City = null,
    string? Address1Stateorprovince = null,
    string? Address1Postalcode = null,
    string? Address1Line1 = null,
    string? Telephone1 = null,
    string? Fax = null,
    string? Address1Country = null,
    string? NewCuit = null,
    string? NewBloqueado = null,
    string? NewRubro = null,
    string? NewIibb = null,
    string? Emailaddress1 = null,
    int? Customertypecode = null,
    string? NewGproveedor = null,
    string? Cbu = null
);

