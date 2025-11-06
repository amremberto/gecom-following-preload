using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocieties;

/// <summary>
/// Query to get all societies.
/// </summary>
public sealed record GetAllSocietiesQuery : IQuery<IEnumerable<SocietyResponse>>;

