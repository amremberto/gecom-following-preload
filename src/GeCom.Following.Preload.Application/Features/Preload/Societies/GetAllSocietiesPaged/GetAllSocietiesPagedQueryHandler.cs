using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocietiesPaged;

/// <summary>
/// Handler para <see cref="GetAllSocietiesPagedQuery"/>.
/// </summary>
internal sealed class GetAllSocietiesPagedQueryHandler : IQueryHandler<GetAllSocietiesPagedQuery, PagedResponse<SocietyResponse>>
{
    private readonly ISocietyRepository _societyRepository;

    public GetAllSocietiesPagedQueryHandler(ISocietyRepository societyRepository)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    public async Task<Result<PagedResponse<SocietyResponse>>> Handle(GetAllSocietiesPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Preloads.Societies.Society> Items, int TotalCount) page =
            await _societyRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        IReadOnlyList<SocietyResponse> mapped = page.Items
            .Select(SocietyMappings.ToResponse)
            .ToList();

        PagedResponse<SocietyResponse> response = new(mapped, page.TotalCount, request.Page, request.PageSize);

        return Result.Success(response);
    }
}


