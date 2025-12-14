using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProviderSocietiesByProviderCuit;

/// <summary>
/// Handler for the GetProviderSocietiesByProviderCuitQuery.
/// </summary>
internal sealed class GetProviderSocietiesByProviderCuitQueryHandler : IQueryHandler<GetProviderSocietiesByProviderCuitQuery, IEnumerable<ProviderSocietyResponse>>
{
    private readonly ISapAccountRepository _sapAccountRepository;
    private readonly ISapProviderSocietiyRepository _sapProviderSocietiyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProviderSocietiesByProviderCuitQueryHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    /// <param name="sapProviderSocietiyRepository">The SAP provider society repository.</param>
    public GetProviderSocietiesByProviderCuitQueryHandler(
        ISapAccountRepository sapAccountRepository,
        ISapProviderSocietiyRepository sapProviderSocietiyRepository)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
        _sapProviderSocietiyRepository = sapProviderSocietiyRepository ?? throw new ArgumentNullException(nameof(sapProviderSocietiyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ProviderSocietyResponse>>> Handle(
        GetProviderSocietiesByProviderCuitQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProviderCuit);

        // Step 1: Get the provider's AccountNumber from SapAccount using CUIT
        // customertypecode = 11 for providers
        SapAccount? providerAccount = await _sapAccountRepository.FirstOrDefaultAsync(
            a => a.NewCuit == request.ProviderCuit && a.Customertypecode == 11,
            cancellationToken);

        if (providerAccount is null)
        {
            return Result.Success(Enumerable.Empty<ProviderSocietyResponse>());
        }

        string providerAccountNumber = providerAccount.Accountnumber;

        // Step 2: Get all relationships in SapProviderSocietiy where Proveedor = provider's AccountNumber
        IReadOnlyList<string> sociedadFiList = await _sapProviderSocietiyRepository
            .GetSocietyFiByProviderAccountNumberAsync(providerAccountNumber, cancellationToken);

        if (sociedadFiList.Count == 0)
        {
            return Result.Success(Enumerable.Empty<ProviderSocietyResponse>());
        }

        // Step 3: Get CUIT and Name from SapAccount for each Sociedadfi
        // AccountNumber = Sociedadfi and customertypecode = 3 for societies
        // Filter out societies without CUIT
        //
        // IMPORTANT: The repository method GetByAccountNumbersAndCustomerTypeAsync uses explicit OR conditions
        // to avoid OPENJSON issues with SQL Server. See docs/SQL-SERVER-OPENJSON-ISSUE.md for details.
        //
        // The repository handles the Expression Tree construction internally to generate SQL compatible
        // with all SQL Server versions:
        //   WHERE accountnumber = 'VAL1' OR accountnumber = 'VAL2' OR accountnumber = 'VAL3'
        //
        // Instead of:
        //   WHERE accountnumber IN (SELECT value FROM OPENJSON('["VAL1","VAL2","VAL3"]'))
        IReadOnlyList<SapAccount> societyAccounts = await _sapAccountRepository
            .GetByAccountNumbersAndCustomerTypeAsync(
                sociedadFiList,
                customerTypeCode: 3,
                cancellationToken);

        var societies = societyAccounts
            .Select(a => new ProviderSocietyResponse(
                a.NewCuit!,
                a.Name))
            .ToList();

        return Result.Success(societies.AsEnumerable());
    }
}
