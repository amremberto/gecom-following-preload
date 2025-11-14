using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.UpdateSapAccount;

/// <summary>
/// Handler for the UpdateSapAccountCommand.
/// </summary>
internal sealed class UpdateSapAccountCommandHandler : ICommandHandler<UpdateSapAccountCommand, SapAccountResponse>
{
    private readonly ISapAccountRepository _sapAccountRepository;
    private readonly ISpdSapUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSapAccountCommandHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    /// <param name="unitOfWork">The SpdSap unit of work.</param>
    public UpdateSapAccountCommandHandler(ISapAccountRepository sapAccountRepository, ISpdSapUnitOfWork unitOfWork)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<SapAccountResponse>> Handle(UpdateSapAccountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el SapAccount existe (con tracking para actualización)
        SapAccount? account = await _sapAccountRepository.GetByAccountNumberForUpdateAsync(request.Accountnumber, cancellationToken);
        if (account is null)
        {
            return Result.Failure<SapAccountResponse>(
                Error.NotFound(
                    "SapAccount.NotFound",
                    $"SAP account with account number '{request.Accountnumber}' was not found."));
        }

        // Actualizar los campos
        account.Name = request.Name;
        account.Address1City = request.Address1City;
        account.Address1Stateorprovince = request.Address1Stateorprovince;
        account.Address1Postalcode = request.Address1Postalcode;
        account.Address1Line1 = request.Address1Line1;
        account.Telephone1 = request.Telephone1;
        account.Fax = request.Fax;
        account.Address1Country = request.Address1Country;
        account.NewCuit = request.NewCuit;
        account.NewBloqueado = request.NewBloqueado;
        account.NewRubro = request.NewRubro;
        account.NewIibb = request.NewIibb;
        account.Emailaddress1 = request.Emailaddress1;
        account.Customertypecode = request.Customertypecode;
        account.NewGproveedor = request.NewGproveedor;
        account.Cbu = request.Cbu;

        // Actualizar en el repositorio
        SapAccount updatedAccount = await _sapAccountRepository.UpdateAsync(account, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        SapAccountResponse response = SapAccountMappings.ToResponse(updatedAccount);

        return Result.Success(response);
    }
}

