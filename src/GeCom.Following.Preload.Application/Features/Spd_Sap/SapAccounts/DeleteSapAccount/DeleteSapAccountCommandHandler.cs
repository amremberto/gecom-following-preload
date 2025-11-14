using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.DeleteSapAccount;

/// <summary>
/// Handler for the DeleteSapAccountCommand.
/// </summary>
internal sealed class DeleteSapAccountCommandHandler : ICommandHandler<DeleteSapAccountCommand>
{
    private readonly ISapAccountRepository _sapAccountRepository;
    private readonly ISpdSapUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSapAccountCommandHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    /// <param name="unitOfWork">The SpdSap unit of work.</param>
    public DeleteSapAccountCommandHandler(ISapAccountRepository sapAccountRepository, ISpdSapUnitOfWork unitOfWork)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteSapAccountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el SapAccount existe
        SapAccount? account = await _sapAccountRepository.GetByAccountNumberAsync(request.Accountnumber, cancellationToken);
        if (account is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "SapAccount.NotFound",
                    $"SAP account with account number '{request.Accountnumber}' was not found."));
        }

        // Eliminar el SapAccount
        await _sapAccountRepository.RemoveByAccountNumberAsync(request.Accountnumber, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

