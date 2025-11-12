using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Providers;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.SearchProviders;

/// <summary>
/// Query to search for providers by a search text.
/// </summary>
/// <param name="SearchText"></param>
public sealed record SearchProvidersQuery(string SearchText) : IQuery<IEnumerable<ProviderResponse>>;
