using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Currencies;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.CreateCurrency;

/// <summary>
/// Command to create a new currency.
/// </summary>
public sealed record CreateCurrencyCommand(
    string Codigo,
    string Descripcion,
    string? CodigoAfip) : ICommand<CurrencyResponse>;

