using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyById;

/// <summary>
/// Handler for the GetCurrencyByIdQuery.
/// </summary>
internal sealed class GetCurrencyByIdQueryHandler : IQueryHandler<GetCurrencyByIdQuery, CurrencyResponse>
{
    private readonly ICurrencyRepository _currencyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrencyByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="currencyRepository">The currency repository.</param>
    public GetCurrencyByIdQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<CurrencyResponse>> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Currency? currency = await _currencyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<CurrencyResponse>(
                Error.NotFound(
                    "Currency.NotFound",
                    $"Currency with ID '{request.Id}' was not found."));
        }

        CurrencyResponse response = CurrencyMappings.ToResponse(currency);

        return Result.Success(response);
    }
}

