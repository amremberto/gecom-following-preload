using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.DeleteCurrency;

/// <summary>
/// Handler for the DeleteCurrencyCommand.
/// </summary>
internal sealed class DeleteCurrencyCommandHandler : ICommandHandler<DeleteCurrencyCommand>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCurrencyCommandHandler"/> class.
    /// </summary>
    /// <param name="currencyRepository">The currency repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteCurrencyCommandHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si la Currency existe
        Currency? currency = await _currencyRepository.GetByIdAsync(request.Id, cancellationToken);
        if (currency is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "Currency.NotFound",
                    $"Currency with ID '{request.Id}' was not found."));
        }

        // Eliminar la Currency
        await _currencyRepository.RemoveByIdAsync(request.Id, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

