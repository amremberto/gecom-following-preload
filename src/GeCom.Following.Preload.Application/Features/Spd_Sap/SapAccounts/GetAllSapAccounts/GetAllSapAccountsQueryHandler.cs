using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccounts;

/// <summary>
/// Handler for the GetAllSapAccountsQuery.
/// </summary>
internal sealed class GetAllSapAccountsQueryHandler : IQueryHandler<GetAllSapAccountsQuery, IEnumerable<SapAccountResponse>>
{
    private readonly ISapAccountRepository _sapAccountRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllSapAccountsQueryHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    public GetAllSapAccountsQueryHandler(ISapAccountRepository sapAccountRepository)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SapAccountResponse>>> Handle(GetAllSapAccountsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Spd_Sap.SapAccounts.SapAccount> accounts = await _sapAccountRepository.GetAllAsync(cancellationToken);

        IEnumerable<SapAccountResponse> response = accounts.Select(SapAccountMappings.ToResponse);

        return Result.Success(response);
    }
}

