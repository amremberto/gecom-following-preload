using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentDetails.GetPaymentDetailById;

/// <summary>
/// Handler for the GetPaymentDetailByIdQuery.
/// </summary>
internal sealed class GetPaymentDetailByIdQueryHandler : IQueryHandler<GetPaymentDetailByIdQuery, PaymentDetailResponse>
{
    private readonly IPaymentDetailRepository _paymentDetailRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPaymentDetailByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="paymentDetailRepository">The payment detail repository.</param>
    public GetPaymentDetailByIdQueryHandler(IPaymentDetailRepository paymentDetailRepository)
    {
        _paymentDetailRepository = paymentDetailRepository ?? throw new ArgumentNullException(nameof(paymentDetailRepository));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentDetailResponse>> Handle(GetPaymentDetailByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        PaymentDetail? paymentDetail = await _paymentDetailRepository.GetByIdAsync(request.Id, cancellationToken);

        if (paymentDetail is null)
        {
            return Result.Failure<PaymentDetailResponse>(
                Error.NotFound(
                    "PaymentDetail.NotFound",
                    $"Payment detail with ID '{request.Id}' was not found."));
        }

        PaymentDetailResponse response = PaymentDetailMappings.ToResponse(paymentDetail);

        return Result.Success(response);
    }
}
