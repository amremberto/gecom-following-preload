using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietiesForCurrentUser;

/// <summary>
/// Query to get societies for the current user based on their role.
/// Determines filtering strategy based on user role:
/// - Provider: Returns societies that the provider can assign documents to
/// - Society: Returns societies that the user has access to
/// - Administrator/ReadOnly: Returns empty result (societies are not needed for these roles in edit context)
/// </summary>
/// <param name="UserRoles">List of user roles.</param>
/// <param name="UserEmail">User email (required for Society role).</param>
/// <param name="ProviderCuit">Provider CUIT (required for Provider role).</param>
public sealed record GetSocietiesForCurrentUserQuery(
    IReadOnlyList<string> UserRoles,
    string? UserEmail,
    string? ProviderCuit) : IQuery<IEnumerable<SocietySelectItemResponse>>;

