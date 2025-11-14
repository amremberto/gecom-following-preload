using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccountsPaged;

/// <summary>
/// Handler for the GetAllSapAccountsPagedQuery.
/// </summary>
internal sealed class GetAllSapAccountsPagedQueryHandler : IQueryHandler<GetAllSapAccountsPagedQuery, PagedResponse<SapAccountResponse>>
{
    private readonly ISapAccountRepository _sapAccountRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllSapAccountsPagedQueryHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    public GetAllSapAccountsPagedQueryHandler(ISapAccountRepository sapAccountRepository)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
    }

    /// <inheritdoc />
    public async Task<Result<PagedResponse<SapAccountResponse>>> Handle(GetAllSapAccountsPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Spd_Sap.SapAccounts.SapAccount> Items, int TotalCount) page =
            await _sapAccountRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        IReadOnlyList<SapAccountResponse> mapped = page.Items
            .Select(SapAccountMappings.ToResponse)
            .ToList();

        PagedResponse<SapAccountResponse> response = new(mapped, page.TotalCount, request.Page, request.PageSize);

        return Result.Success(response);
    }
}

