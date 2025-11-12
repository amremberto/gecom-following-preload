using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderById;

/// <summary>
/// Handler for the GetProviderByIdQuery.
/// </summary>
internal sealed class GetProviderByIdQueryHandler : IQueryHandler<GetProviderByIdQuery, ProviderResponse>
{
    private readonly IProviderRepository _providerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProviderByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="providerRepository">The provider repository.</param>
    public GetProviderByIdQueryHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
    }

    /// <inheritdoc />
    public async Task<Result<ProviderResponse>> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.Providers.Provider? provider = await _providerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (provider is null)
        {
            return Result.Failure<ProviderResponse>(
                Error.NotFound(
                    "Provider.NotFound",
                    $"Provider with ID '{request.Id}' was not found."));
        }

        ProviderResponse response = ProviderMappings.ToResponse(provider);

        return Result.Success(response);
    }
}

