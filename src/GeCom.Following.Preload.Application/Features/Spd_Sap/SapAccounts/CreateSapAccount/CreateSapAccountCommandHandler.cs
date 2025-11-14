using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.CreateSapAccount;

/// <summary>
/// Handler for the CreateSapAccountCommand.
/// </summary>
internal sealed class CreateSapAccountCommandHandler : ICommandHandler<CreateSapAccountCommand, SapAccountResponse>
{
    private readonly ISapAccountRepository _sapAccountRepository;
    private readonly ISpdSapUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSapAccountCommandHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    /// <param name="unitOfWork">The SpdSap unit of work.</param>
    public CreateSapAccountCommandHandler(ISapAccountRepository sapAccountRepository, ISpdSapUnitOfWork unitOfWork)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<SapAccountResponse>> Handle(CreateSapAccountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si ya existe un SapAccount con el mismo account number
        SapAccount? existingByAccountNumber = await _sapAccountRepository.GetByAccountNumberAsync(request.Accountnumber, cancellationToken);
        if (existingByAccountNumber is not null)
        {
            return Result.Failure<SapAccountResponse>(
                Error.Conflict(
                    "SapAccount.Conflict",
                    $"A SAP account with account number '{request.Accountnumber}' already exists."));
        }

        // Crear la nueva entidad SapAccount
        SapAccount sapAccount = new()
        {
            Accountnumber = request.Accountnumber,
            Name = request.Name,
            Address1City = request.Address1City,
            Address1Stateorprovince = request.Address1Stateorprovince,
            Address1Postalcode = request.Address1Postalcode,
            Address1Line1 = request.Address1Line1,
            Telephone1 = request.Telephone1,
            Fax = request.Fax,
            Address1Country = request.Address1Country,
            NewCuit = request.NewCuit,
            NewBloqueado = request.NewBloqueado,
            NewRubro = request.NewRubro,
            NewIibb = request.NewIibb,
            Emailaddress1 = request.Emailaddress1,
            Customertypecode = request.Customertypecode,
            NewGproveedor = request.NewGproveedor,
            Cbu = request.Cbu
        };

        // Agregar al repositorio
        SapAccount addedAccount = await _sapAccountRepository.AddAsync(sapAccount, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        SapAccountResponse response = SapAccountMappings.ToResponse(addedAccount);

        return Result.Success(response);
    }
}

