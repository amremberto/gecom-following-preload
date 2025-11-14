using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccountsPaged;

/// <summary>
/// Query to get SAP accounts with pagination.
/// </summary>
public sealed record GetAllSapAccountsPagedQuery(int Page, int PageSize) : IQuery<PagedResponse<SapAccountResponse>>;

