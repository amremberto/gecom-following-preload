using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.CreatePaymentType;

/// <summary>
/// Command to create a new payment type.
/// </summary>
public sealed record CreatePaymentTypeCommand(
    string Descripcion) : ICommand<PaymentTypeResponse>;

