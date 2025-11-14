using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetAllPaymentTypes;

/// <summary>
/// Query to get all payment types.
/// </summary>
public sealed record GetAllPaymentTypesQuery : IQuery<IEnumerable<PaymentTypeResponse>>;

