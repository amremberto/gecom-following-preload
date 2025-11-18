namespace GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments.Create;

/// <summary>
/// Request DTO for creating a user society assignment.
/// </summary>
public sealed record CreateUserSocietyAssignmentRequest(
    string Email,
    string CuitClient,
    string SociedadFi
);

