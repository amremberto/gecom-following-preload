namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;

/// <summary>
/// Response DTO for SapAccount.
/// </summary>
public sealed record SapAccountResponse(
    string Accountnumber,
    string? Name,
    string? Address1City,
    string? Address1Stateorprovince,
    string? Address1Postalcode,
    string? Address1Line1,
    string? Telephone1,
    string? Fax,
    string? Address1Country,
    string? NewCuit,
    string? NewBloqueado,
    string? NewRubro,
    string? NewIibb,
    string? Emailaddress1,
    int? Customertypecode,
    string? NewGproveedor,
    string? Cbu
);

