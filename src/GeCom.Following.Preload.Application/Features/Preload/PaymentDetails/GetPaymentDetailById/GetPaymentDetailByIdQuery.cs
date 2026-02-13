using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentDetails;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentDetails.GetPaymentDetailById;

/// <summary>
/// Query to get a payment detail by its ID.
/// </summary>
public sealed record GetPaymentDetailByIdQuery(int Id) : IQuery<PaymentDetailResponse>;
