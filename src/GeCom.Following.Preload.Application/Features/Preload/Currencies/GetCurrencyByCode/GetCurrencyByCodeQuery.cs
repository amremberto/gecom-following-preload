using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Currencies;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyByCode;

/// <summary>
/// Query to get a currency by its code.
/// </summary>
public sealed record GetCurrencyByCodeQuery(string Codigo) : IQuery<CurrencyResponse>;

