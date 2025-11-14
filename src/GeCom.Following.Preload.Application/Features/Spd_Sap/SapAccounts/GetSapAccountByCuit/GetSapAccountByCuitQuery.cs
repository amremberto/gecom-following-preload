using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByCuit;

/// <summary>
/// Query to get a SAP account by its CUIT.
/// </summary>
public sealed record GetSapAccountByCuitQuery(string Cuit) : IQuery<SapAccountResponse>;

