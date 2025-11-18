namespace GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments.GetAll;

/// <summary>
/// Request DTO for getting all user society assignments (prepared for future pagination).
/// </summary>
public sealed record GetAllUserSocietyAssignmentsRequest(int? Page = null, int? PageSize = null);

