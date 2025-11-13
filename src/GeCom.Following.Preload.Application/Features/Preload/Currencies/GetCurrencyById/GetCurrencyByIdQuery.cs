using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Currencies;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetCurrencyById;

/// <summary>
/// Query to get a currency by its ID.
/// </summary>
public sealed record GetCurrencyByIdQuery(int Id) : IQuery<CurrencyResponse>;

