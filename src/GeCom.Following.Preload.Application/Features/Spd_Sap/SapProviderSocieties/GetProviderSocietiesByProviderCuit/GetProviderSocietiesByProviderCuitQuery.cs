using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProviderSocietiesByProviderCuit;

/// <summary>
/// Query to get all societies that a provider can assign documents to.
/// </summary>
/// <param name="ProviderCuit">The provider CUIT.</param>
public sealed record GetProviderSocietiesByProviderCuitQuery(string ProviderCuit) : IQuery<IEnumerable<ProviderSocietyResponse>>;
