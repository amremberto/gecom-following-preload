namespace GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;

/// <summary>
/// Response DTO for UserSocietyAssignment.
/// </summary>
public sealed record UserSocietyAssignmentResponse(
    int Id,
    string Email,
    string CuitClient,
    string SociedadFi
);

