using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.Domain.Preloads.Providers;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetAllProviders;

/// <summary>
/// Handler for the GetAllProvidersQuery.
/// </summary>
internal sealed class GetAllProvidersQueryHandler : IQueryHandler<GetAllProvidersQuery, IEnumerable<ProviderResponse>>
{
    private readonly IProviderRepository _providerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllProvidersQueryHandler"/> class.
    /// </summary>
    /// <param name="providerRepository">The provider repository.</param>
    public GetAllProvidersQueryHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ProviderResponse>>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Provider> providers = await _providerRepository.GetAllAsync(cancellationToken);

        IEnumerable<ProviderResponse> response = providers.Select(ProviderMappings.ToResponse);

        return Result.Success(response);
    }
}

