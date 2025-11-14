using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.UpdateSapAccount;

/// <summary>
/// Command to update an existing SAP account.
/// </summary>
public sealed record UpdateSapAccountCommand(
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
    string? Cbu) : ICommand<SapAccountResponse>;

