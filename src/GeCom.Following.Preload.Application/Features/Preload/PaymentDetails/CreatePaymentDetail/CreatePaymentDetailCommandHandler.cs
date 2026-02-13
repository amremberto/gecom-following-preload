using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentDetails.CreatePaymentDetail;

/// <summary>
/// Handler for the CreatePaymentDetailCommand.
/// </summary>
internal sealed class CreatePaymentDetailCommandHandler : ICommandHandler<CreatePaymentDetailCommand, PaymentDetailResponse>
{
    private readonly IPaymentDetailRepository _paymentDetailRepository;
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePaymentDetailCommandHandler"/> class.
    /// </summary>
    /// <param name="paymentDetailRepository">The payment detail repository.</param>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreatePaymentDetailCommandHandler(
        IPaymentDetailRepository paymentDetailRepository,
        IPaymentTypeRepository paymentTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentDetailRepository = paymentDetailRepository ?? throw new ArgumentNullException(nameof(paymentDetailRepository));
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentDetailResponse>> Handle(CreatePaymentDetailCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        PaymentType? paymentType = await _paymentTypeRepository.GetByIdAsync(request.IdTipoDePago, cancellationToken);
        if (paymentType is null)
        {
            return Result.Failure<PaymentDetailResponse>(
                Error.NotFound(
                    "PaymentType.NotFound",
                    $"Payment type with ID '{request.IdTipoDePago}' was not found."));
        }

        DateOnly fechaAlta = request.FechaAlta ?? DateOnly.FromDateTime(DateTime.Today);

        PaymentDetail paymentDetail = new()
        {
            IdTipoDePago = request.IdTipoDePago,
            NroCheque = request.NroCheque,
            Banco = request.Banco,
            Vencimiento = request.Vencimiento,
            ImporteRecibido = request.ImporteRecibido,
            FechaAlta = fechaAlta,
            NamePdf = request.NamePdf
        };

        PaymentDetail addedPaymentDetail = await _paymentDetailRepository.AddAsync(paymentDetail, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        PaymentDetailResponse response = PaymentDetailMappings.ToResponse(addedPaymentDetail);

        return Result.Success(response);
    }
}
