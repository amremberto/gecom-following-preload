using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Providers;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderById;

/// <summary>
/// Query to get a provider by its ID.
/// </summary>
public sealed record GetProviderByIdQuery(int Id) : IQuery<ProviderResponse>;

