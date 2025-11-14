using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByAccountNumber;

/// <summary>
/// Handler for the GetSapAccountByAccountNumberQuery.
/// </summary>
internal sealed class GetSapAccountByAccountNumberQueryHandler : IQueryHandler<GetSapAccountByAccountNumberQuery, SapAccountResponse>
{
    private readonly ISapAccountRepository _sapAccountRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSapAccountByAccountNumberQueryHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    public GetSapAccountByAccountNumberQueryHandler(ISapAccountRepository sapAccountRepository)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
    }

    /// <inheritdoc />
    public async Task<Result<SapAccountResponse>> Handle(GetSapAccountByAccountNumberQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        SapAccount? account = await _sapAccountRepository.GetByAccountNumberAsync(request.Accountnumber, cancellationToken);

        if (account is null)
        {
            return Result.Failure<SapAccountResponse>(
                Error.NotFound(
                    "SapAccount.NotFound",
                    $"SAP account with account number '{request.Accountnumber}' was not found."));
        }

        SapAccountResponse response = SapAccountMappings.ToResponse(account);

        return Result.Success(response);
    }
}

