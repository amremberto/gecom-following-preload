using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.Domain.Preloads.Providers;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.SearchProviders;

internal sealed class SearchProvidersQueryHandler : IQueryHandler<SearchProvidersQuery, IEnumerable<ProviderResponse>>
{
    private readonly IProviderRepository _providerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllProvidersQueryHandler"/> class.
    /// </summary>
    /// <param name="providerRepository">The provider repository.</param>
    public SearchProvidersQueryHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
    }

    public async Task<Result<IEnumerable<ProviderResponse>>> Handle(SearchProvidersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Provider> providers = await _providerRepository.SearchAsync(request.SearchText, 20, cancellationToken);

        IEnumerable<ProviderResponse> response = providers.Select(ProviderMappings.ToResponse);

        return Result.Success(response);
    }
}
