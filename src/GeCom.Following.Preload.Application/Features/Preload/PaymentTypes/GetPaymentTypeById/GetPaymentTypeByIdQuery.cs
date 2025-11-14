using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeById;

/// <summary>
/// Query to get a payment type by its ID.
/// </summary>
public sealed record GetPaymentTypeByIdQuery(int Id) : IQuery<PaymentTypeResponse>;

