using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyByCode;

/// <summary>
/// Handler for the GetCurrencyByCodeQuery.
/// </summary>
internal sealed class GetCurrencyByCodeQueryHandler : IQueryHandler<GetCurrencyByCodeQuery, CurrencyResponse>
{
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrencyByCodeQueryHandler"/> class.
    /// </summary>
    /// <param name="currencyRepository">The currency repository.</param>
    public GetCurrencyByCodeQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<CurrencyResponse>> Handle(GetCurrencyByCodeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Currency? currency = await _currencyRepository.GetByCodeAsync(request.Codigo, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<CurrencyResponse>(
                Error.NotFound(
                    "Currency.NotFound",
                    $"Currency with code '{request.Codigo}' was not found."));
        }

        CurrencyResponse response = CurrencyMappings.ToResponse(currency);

        return Result.Success(response);
    }
}

