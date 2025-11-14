using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.DeletePaymentType;

/// <summary>
/// Command to delete a payment type by its ID.
/// </summary>
public sealed record DeletePaymentTypeCommand(int Id) : ICommand;

