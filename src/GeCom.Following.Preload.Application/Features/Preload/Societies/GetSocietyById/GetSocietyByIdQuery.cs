using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyById;

/// <summary>
/// Query to get a society by its ID.
/// </summary>
public sealed record GetSocietyByIdQuery(int Id) : IQuery<SocietyResponse>;

