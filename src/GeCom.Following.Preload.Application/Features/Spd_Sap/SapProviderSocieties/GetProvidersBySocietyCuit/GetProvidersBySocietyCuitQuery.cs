using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Providers;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProvidersBySocietyCuit;

/// <summary>
/// Query to get all providers that can assign documents to a specific society.
/// </summary>
/// <param name="SocietyCuit">The society CUIT.</param>
public sealed record GetProvidersBySocietyCuitQuery(string SocietyCuit) : IQuery<IEnumerable<ProviderSelectItemResponse>>;

