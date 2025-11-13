using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Currencies;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.UpdateCurrency;

/// <summary>
/// Command to update an existing currency.
/// </summary>
public sealed record UpdateCurrencyCommand(
    int MonedaId,
    string Codigo,
    string Descripcion,
    string? CodigoAfip) : ICommand<CurrencyResponse>;

