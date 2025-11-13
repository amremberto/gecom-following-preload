using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrencies;

/// <summary>
/// Handler for the GetAllCurrenciesQuery.
/// </summary>
internal sealed class GetAllCurrenciesQueryHandler : IQueryHandler<GetAllCurrenciesQuery, IEnumerable<CurrencyResponse>>
{
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllCurrenciesQueryHandler"/> class.
    /// </summary>
    /// <param name="currencyRepository">The currency repository.</param>
    public GetAllCurrenciesQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<CurrencyResponse>>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Currency> currencies = await _currencyRepository.GetAllAsync(cancellationToken);

        IEnumerable<CurrencyResponse> response = currencies.Select(CurrencyMappings.ToResponse);

        return Result.Success(response);
    }
}

