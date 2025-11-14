using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.DeletePaymentType;

/// <summary>
/// Handler for the DeletePaymentTypeCommand.
/// </summary>
internal sealed class DeletePaymentTypeCommandHandler : ICommandHandler<DeletePaymentTypeCommand>
{
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePaymentTypeCommandHandler"/> class.
    /// </summary>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeletePaymentTypeCommandHandler(
        IPaymentTypeRepository paymentTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeletePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el PaymentType existe
        PaymentType? paymentType = await _paymentTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentType is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "PaymentType.NotFound",
                    $"Payment type with ID '{request.Id}' was not found."));
        }

        // Eliminar el PaymentType
        await _paymentTypeRepository.RemoveByIdAsync(request.Id, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

