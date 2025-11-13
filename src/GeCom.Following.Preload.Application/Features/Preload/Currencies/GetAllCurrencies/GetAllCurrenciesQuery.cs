using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Currencies;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrencies;

/// <summary>
/// Query to get all currencies.
/// </summary>
public sealed record GetAllCurrenciesQuery : IQuery<IEnumerable<CurrencyResponse>>;

