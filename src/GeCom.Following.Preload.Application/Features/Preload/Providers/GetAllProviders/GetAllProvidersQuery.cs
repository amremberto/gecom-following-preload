using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Providers;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetAllProviders;

/// <summary>
/// Query to get all providers.
/// </summary>
public sealed record GetAllProvidersQuery : IQuery<IEnumerable<ProviderResponse>>;

