using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.DeleteCurrency;

/// <summary>
/// Command to delete a currency by its ID.
/// </summary>
public sealed record DeleteCurrencyCommand(int Id) : ICommand;

