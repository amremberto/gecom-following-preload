using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByAccountNumber;

/// <summary>
/// Query to get a SAP account by its account number.
/// </summary>
public sealed record GetSapAccountByAccountNumberQuery(string Accountnumber) : IQuery<SapAccountResponse>;

