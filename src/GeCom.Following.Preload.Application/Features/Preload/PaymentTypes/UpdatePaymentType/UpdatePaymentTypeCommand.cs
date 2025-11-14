using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.UpdatePaymentType;

/// <summary>
/// Command to update an existing payment type.
/// </summary>
public sealed record UpdatePaymentTypeCommand(
    int Id,
    string Descripcion) : ICommand<PaymentTypeResponse>;

