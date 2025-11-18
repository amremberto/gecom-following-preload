using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;

public partial class UserSocietyAssignment : BaseEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CuitClient { get; set; } = string.Empty;
    public string SociedadFi { get; set; } = string.Empty;
}
