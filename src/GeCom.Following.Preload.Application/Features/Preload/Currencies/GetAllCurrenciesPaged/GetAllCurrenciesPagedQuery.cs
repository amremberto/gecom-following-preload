using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.GetAllCurrenciesPaged;

/// <summary>
/// Query para obtener Currencies paginadas.
/// </summary>
public sealed record GetAllCurrenciesPagedQuery(int Page, int PageSize) : IQuery<PagedResponse<CurrencyResponse>>;

