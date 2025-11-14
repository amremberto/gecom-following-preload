using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeById;

/// <summary>
/// Handler for the GetPaymentTypeByIdQuery.
/// </summary>
internal sealed class GetPaymentTypeByIdQueryHandler : IQueryHandler<GetPaymentTypeByIdQuery, PaymentTypeResponse>
{
    private readonly IPaymentTypeRepository _paymentTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPaymentTypeByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    public GetPaymentTypeByIdQueryHandler(IPaymentTypeRepository paymentTypeRepository)
    {
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentTypeResponse>> Handle(GetPaymentTypeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        PaymentType? paymentType = await _paymentTypeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (paymentType is null)
        {
            return Result.Failure<PaymentTypeResponse>(
                Error.NotFound(
                    "PaymentType.NotFound",
                    $"Payment type with ID '{request.Id}' was not found."));
        }

        PaymentTypeResponse response = PaymentTypeMappings.ToResponse(paymentType);

        return Result.Success(response);
    }
}

