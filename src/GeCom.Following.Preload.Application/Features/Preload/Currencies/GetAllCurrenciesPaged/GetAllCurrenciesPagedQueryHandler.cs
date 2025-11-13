using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrenciesPaged;

/// <summary>
/// Handler para <see cref="GetAllCurrenciesPagedQuery"/>.
/// </summary>
internal sealed class GetAllCurrenciesPagedQueryHandler : IQueryHandler<GetAllCurrenciesPagedQuery, PagedResponse<CurrencyResponse>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetAllCurrenciesPagedQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
    }

    public async Task<Result<PagedResponse<CurrencyResponse>>> Handle(GetAllCurrenciesPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Preloads.Currencies.Currency> Items, int TotalCount) page =
            await _currencyRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        IReadOnlyList<CurrencyResponse> mapped = page.Items
            .Select(CurrencyMappings.ToResponse)
            .ToList();

        PagedResponse<CurrencyResponse> response = new(mapped, page.TotalCount, request.Page, request.PageSize);

        return Result.Success(response);
    }
}

