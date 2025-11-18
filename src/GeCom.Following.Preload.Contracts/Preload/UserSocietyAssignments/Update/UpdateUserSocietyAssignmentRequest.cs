namespace GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments.Update;

/// <summary>
/// Request DTO for updating a user society assignment.
/// </summary>
public sealed record UpdateUserSocietyAssignmentRequest(
    int Id,
    string Email,
    string CuitClient,
    string SociedadFi
);

