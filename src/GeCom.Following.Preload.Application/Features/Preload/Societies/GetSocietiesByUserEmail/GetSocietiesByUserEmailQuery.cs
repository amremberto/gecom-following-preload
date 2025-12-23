using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietiesByUserEmail;

/// <summary>
/// Query to get societies by user email for select dropdowns.
/// </summary>
public sealed record GetSocietiesByUserEmailQuery(string UserEmail) : IQuery<IEnumerable<SocietySelectItem>>;

