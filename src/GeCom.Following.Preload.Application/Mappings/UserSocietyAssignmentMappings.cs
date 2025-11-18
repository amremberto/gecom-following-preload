using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;

namespace GeCom.Following.Preload.Application.Mappings;

/// <summary>
/// Mapping extensions for UserSocietyAssignment entities.
/// </summary>
internal static class UserSocietyAssignmentMappings
{
    /// <summary>
    /// Maps a UserSocietyAssignment entity to a UserSocietyAssignmentResponse DTO.
    /// </summary>
    /// <param name="userSocietyAssignment">The entity to map.</param>
    /// <returns>The mapped response DTO.</returns>
    public static UserSocietyAssignmentResponse ToResponse(UserSocietyAssignment userSocietyAssignment)
    {
        UserSocietyAssignmentResponse result = new(
            userSocietyAssignment.Id,
            userSocietyAssignment.Email,
            userSocietyAssignment.CuitClient,
            userSocietyAssignment.SociedadFi
        );

        return result;
    }
}

