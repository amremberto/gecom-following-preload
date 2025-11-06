using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocieties;

/// <summary>
/// Handler for the GetAllSocietiesQuery.
/// </summary>
internal sealed class GetAllSocietiesQueryHandler : IQueryHandler<GetAllSocietiesQuery, IEnumerable<SocietyResponse>>
{
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllSocietiesQueryHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    public GetAllSocietiesQueryHandler(ISocietyRepository societyRepository)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SocietyResponse>>> Handle(GetAllSocietiesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Society> societies = await _societyRepository.GetAllAsync(cancellationToken);

        IEnumerable<SocietyResponse> response = societies.Select(SocietyMappings.ToResponse);

        return Result.Success(response);
    }
}

