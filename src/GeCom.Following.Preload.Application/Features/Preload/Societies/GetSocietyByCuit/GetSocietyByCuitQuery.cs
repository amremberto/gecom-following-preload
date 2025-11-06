using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCuit;

/// <summary>
/// Query to get a society by its CUIT.
/// </summary>
public sealed record GetSocietyByCuitQuery(string Cuit) : IQuery<SocietyResponse>;

