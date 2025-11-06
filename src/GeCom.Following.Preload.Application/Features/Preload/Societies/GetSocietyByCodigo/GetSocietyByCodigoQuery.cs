using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCodigo;

/// <summary>
/// Query to get a society by its code.
/// </summary>
public sealed record GetSocietyByCodigoQuery(string Codigo) : IQuery<SocietyResponse>;

