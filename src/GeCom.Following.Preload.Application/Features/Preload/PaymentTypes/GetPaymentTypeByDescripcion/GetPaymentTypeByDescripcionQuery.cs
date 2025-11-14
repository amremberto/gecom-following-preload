using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeByDescripcion;

/// <summary>
/// Query to get a payment type by its description.
/// </summary>
public sealed record GetPaymentTypeByDescripcionQuery(string Descripcion) : IQuery<PaymentTypeResponse>;

