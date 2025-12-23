using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProvidersBySocietyCuit;

/// <summary>
/// Handler for the GetProvidersBySocietyCuitQuery.
/// </summary>
internal sealed class GetProvidersBySocietyCuitQueryHandler : IQueryHandler<GetProvidersBySocietyCuitQuery, IEnumerable<ProviderSelectItemResponse>>
{
    private readonly ISapAccountRepository _sapAccountRepository;
    private readonly ISapProviderSocietiyRepository _sapProviderSocietiyRepository;
    private readonly IProviderRepository _providerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProvidersBySocietyCuitQueryHandler"/> class.
    /// </summary>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    /// <param name="sapProviderSocietiyRepository">The SAP provider society repository.</param>
    /// <param name="providerRepository">The provider repository.</param>
    public GetProvidersBySocietyCuitQueryHandler(
        ISapAccountRepository sapAccountRepository,
        ISapProviderSocietiyRepository sapProviderSocietiyRepository,
        IProviderRepository providerRepository)
    {
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
        _sapProviderSocietiyRepository = sapProviderSocietiyRepository ?? throw new ArgumentNullException(nameof(sapProviderSocietiyRepository));
        _providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ProviderSelectItemResponse>>> Handle(
        GetProvidersBySocietyCuitQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SocietyCuit);

        // Step 1: Get the society's AccountNumber from SapAccount using CUIT
        // customertypecode = 3 for societies
        SapAccount? societyAccount = await _sapAccountRepository.FirstOrDefaultAsync(
            a => a.NewCuit == request.SocietyCuit && a.Customertypecode == 3,
            cancellationToken);

        if (societyAccount is null)
        {
            return Result.Success(Enumerable.Empty<ProviderSelectItemResponse>());
        }

        string societyFi = societyAccount.Accountnumber;

        // Step 2: Get all provider account numbers from SapProviderSocietiy where Sociedadfi = society's AccountNumber
        IReadOnlyList<string> providerAccountNumbers = await _sapProviderSocietiyRepository
            .GetProviderAccountNumbersBySocietyFiAsync(societyFi, cancellationToken);

        if (providerAccountNumbers.Count == 0)
        {
            return Result.Success(Enumerable.Empty<ProviderSelectItemResponse>());
        }

        // Step 3: Get CUITs from SapAccount for each provider account number
        // AccountNumber = provider account number and customertypecode = 11 for providers
        // Filter out providers without CUIT
        //
        // IMPORTANT: The repository method GetByAccountNumbersAndCustomerTypeAsync uses explicit OR conditions
        // to avoid OPENJSON issues with SQL Server. See docs/SQL-SERVER-OPENJSON-ISSUE.md for details.
        IReadOnlyList<SapAccount> providerAccounts = await _sapAccountRepository
            .GetByAccountNumbersAndCustomerTypeAsync(
                providerAccountNumbers,
                customerTypeCode: 11,
                cancellationToken);

        if (providerAccounts.Count == 0)
        {
            return Result.Success(Enumerable.Empty<ProviderSelectItemResponse>());
        }

        // Step 4: Extract CUITs from provider accounts
        var providerCuits = providerAccounts
            .Where(a => !string.IsNullOrWhiteSpace(a.NewCuit))
            .Select(a => a.NewCuit!)
            .Distinct()
            .ToList();

        if (providerCuits.Count == 0)
        {
            return Result.Success(Enumerable.Empty<ProviderSelectItemResponse>());
        }

        // Step 5: Get complete provider information from Provider repository
        IReadOnlyList<Domain.Preloads.Providers.Provider> providers = await _providerRepository
            .GetByCuitsAsync(providerCuits, cancellationToken);

        // Step 6: Map to ProviderSelectItemResponse (only CUIT and RazonSocial)
        var providerSelectItems = providers
            .Select(p => new ProviderSelectItemResponse(p.Cuit, p.RazonSocial))
            .OrderBy(p => p.RazonSocial)
            .ToList();

        return Result.Success(providerSelectItems.AsEnumerable());
    }
}

