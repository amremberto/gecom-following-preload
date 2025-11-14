namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts.GetAll;

/// <summary>
/// Request DTO for getting all SAP accounts with pagination.
/// </summary>
public sealed record GetAllSapAccountsRequest(
    int? Page = null,
    int? PageSize = null
);

