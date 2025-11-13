using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.UpdateCurrency;

/// <summary>
/// Handler for the UpdateCurrencyCommand.
/// </summary>
internal sealed class UpdateCurrencyCommandHandler : ICommandHandler<UpdateCurrencyCommand, CurrencyResponse>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCurrencyCommandHandler"/> class.
    /// </summary>
    /// <param name="currencyRepository">The currency repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateCurrencyCommandHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<CurrencyResponse>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si la Currency existe
        Currency? currency = await _currencyRepository.GetByIdAsync(request.MonedaId, cancellationToken);
        if (currency is null)
        {
            return Result.Failure<CurrencyResponse>(
                Error.NotFound(
                    "Currency.NotFound",
                    $"Currency with ID '{request.MonedaId}' was not found."));
        }

        // Verificar si ya existe otra Currency con el mismo código (excluyendo la actual)
        Currency? existingByCodigo = await _currencyRepository.GetByCodeAsync(request.Codigo, cancellationToken);
        if (existingByCodigo is not null && existingByCodigo.MonedaId != request.MonedaId)
        {
            return Result.Failure<CurrencyResponse>(
                Error.Conflict(
                    "Currency.Conflict",
                    $"A currency with code '{request.Codigo}' already exists."));
        }

        // Actualizar los campos
        currency.Codigo = request.Codigo;
        currency.Descripcion = request.Descripcion;
        currency.CodigoAfip = request.CodigoAfip;

        // Actualizar en el repositorio
        Currency updatedCurrency = await _currencyRepository.UpdateAsync(currency, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        CurrencyResponse response = CurrencyMappings.ToResponse(updatedCurrency);

        return Result.Success(response);
    }
}

