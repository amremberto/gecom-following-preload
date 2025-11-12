using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Providers;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderByCuit;

/// <summary>
/// Query to get a provider by its CUIT.
/// </summary>
public sealed record GetProviderByCuitQuery(string Cuit) : IQuery<ProviderResponse>;

