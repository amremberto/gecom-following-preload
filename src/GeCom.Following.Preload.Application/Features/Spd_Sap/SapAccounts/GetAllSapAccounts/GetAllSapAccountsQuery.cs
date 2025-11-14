using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccounts;

/// <summary>
/// Query to get all SAP accounts.
/// </summary>
public sealed record GetAllSapAccountsQuery() : IQuery<IEnumerable<SapAccountResponse>>;

