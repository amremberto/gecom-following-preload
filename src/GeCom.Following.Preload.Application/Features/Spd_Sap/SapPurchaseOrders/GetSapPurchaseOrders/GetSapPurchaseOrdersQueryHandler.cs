using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrders;

/// <summary>
/// Handler for the GetSapPurchaseOrdersQuery.
/// Determines filtering strategy based on user role:
/// - Administrator/ReadOnly: Returns all purchase orders
/// - Providers: Returns purchase orders filtered by provider account number (obtained from CUIT)
/// - Societies: Returns purchase orders filtered by Sociedadfi codes from user's assigned societies
/// </summary>
internal sealed class GetSapPurchaseOrdersQueryHandler
    : IQueryHandler<GetSapPurchaseOrdersQuery, IEnumerable<SapPurchaseOrderResponse>>
{
    private readonly ISapPurchaseOrderRepository _sapPurchaseOrderRepository;
    private readonly ISapAccountRepository _sapAccountRepository;
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSapPurchaseOrdersQueryHandler"/> class.
    /// </summary>
    /// <param name="sapPurchaseOrderRepository">The SAP purchase order repository.</param>
    /// <param name="sapAccountRepository">The SAP account repository.</param>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetSapPurchaseOrdersQueryHandler(
        ISapPurchaseOrderRepository sapPurchaseOrderRepository,
        ISapAccountRepository sapAccountRepository,
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _sapPurchaseOrderRepository = sapPurchaseOrderRepository ?? throw new ArgumentNullException(nameof(sapPurchaseOrderRepository));
        _sapAccountRepository = sapAccountRepository ?? throw new ArgumentNullException(nameof(sapAccountRepository));
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SapPurchaseOrderResponse>>> Handle(
        GetSapPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Spd_Sap.SapPurchaseOrders.SapPurchaseOrder> purchaseOrders;

        // Determine filtering strategy based on user roles
        // Using role constants directly to avoid dependency on WebApi layer
        const string followingAdministrator = "Following.Administrator";
        const string followingPreloadReadOnly = "Following.Preload.ReadOnly";
        const string followingPreloadProviders = "Following.Preload.Providers";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        if (HasRole(request.UserRoles, followingAdministrator) ||
            HasRole(request.UserRoles, followingPreloadReadOnly))
        {
            // Administrator or ReadOnly: Return all purchase orders
            purchaseOrders = await _sapPurchaseOrderRepository.GetAllAsync(cancellationToken);
        }
        else if (HasRole(request.UserRoles, followingPreloadProviders))
        {
            // Providers: Filter by provider account number obtained from CUIT
            if (string.IsNullOrWhiteSpace(request.ProviderCuit))
            {
                return Result.Failure<IEnumerable<SapPurchaseOrderResponse>>(
                    Error.Failure(
                        "SapPurchaseOrder.ProviderCuitRequired",
                        "Provider CUIT is required for users with Providers role."));
            }

            // Get the provider's AccountNumber from SapAccount using CUIT
            // customertypecode = 11 for providers
            SapAccount? providerAccount = await _sapAccountRepository.FirstOrDefaultAsync(
                a => a.NewCuit == request.ProviderCuit && a.Customertypecode == 11,
                cancellationToken);

            if (providerAccount is null)
            {
                // Provider not found, return empty result
                purchaseOrders = [];
            }
            else
            {
                // Get purchase orders filtered by provider account number
                purchaseOrders = await _sapPurchaseOrderRepository.GetByProviderAccountNumberAsync(
                    providerAccount.Accountnumber,
                    cancellationToken);
            }
        }
        else if (HasRole(request.UserRoles, followingPreloadSocieties))
        {
            // Societies: Filter by Sociedadfi codes from SapProviderSocietiy for societies user can upload documents to
            if (string.IsNullOrWhiteSpace(request.UserEmail))
            {
                return Result.Failure<IEnumerable<SapPurchaseOrderResponse>>(
                    Error.Failure(
                        "SapPurchaseOrder.UserEmailRequired",
                        "User email is required for users with Societies role."));
            }

            // Get all society assignments for the user
            IEnumerable<UserSocietyAssignment> assignments =
                await _userSocietyAssignmentRepository.GetByEmailAsync(request.UserEmail, cancellationToken);

            var societyCuits = assignments
                .Select(a => a.CuitClient)
                .Distinct()
                .ToList();

            if (societyCuits.Count == 0)
            {
                // User has no society assignments, return empty result
                purchaseOrders = [];
            }
            else
            {
                // Get AccountNumbers for societies using CUITs (customertypecode = 3 for societies)
                // We need to find SapAccounts where NewCuit matches the society CUITs
                var societyAccounts = new List<SapAccount>();
                foreach (string societyCuit in societyCuits)
                {
                    SapAccount? account = await _sapAccountRepository.FirstOrDefaultAsync(
                        a => a.NewCuit == societyCuit && a.Customertypecode == 3,
                        cancellationToken);
                    if (account is not null)
                    {
                        societyAccounts.Add(account);
                    }
                }

                if (societyAccounts.Count == 0)
                {
                    purchaseOrders = [];
                }
                else
                {
                    // Get Sociedadfi codes from SapProviderSocietiy where Sociedadfi matches society AccountNumbers
                    // The Sociedadfi field in SapProviderSocietiy corresponds to the AccountNumber of societies
                    var sociedadFiCodes = societyAccounts
                        .Where(a => !string.IsNullOrWhiteSpace(a.Accountnumber))
                        .Select(a => a.Accountnumber)
                        .Distinct()
                        .ToList();

                    if (sociedadFiCodes.Count == 0)
                    {
                        purchaseOrders = [];
                    }
                    else
                    {
                        // Get purchase orders filtered by Sociedadfi codes (codigosociedadfi)
                        purchaseOrders = await _sapPurchaseOrderRepository.GetBySociedadFiCodesAsync(
                            sociedadFiCodes,
                            cancellationToken);
                    }
                }
            }
        }
        else
        {
            // Unknown role: Return empty result for security
            purchaseOrders = [];
        }

        IEnumerable<SapPurchaseOrderResponse> response = purchaseOrders.Select(SapPurchaseOrderMappings.ToResponse);

        return Result.Success(response);
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="userRoles">List of user roles.</param>
    /// <param name="role">Role to check.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    private static bool HasRole(IReadOnlyList<string> userRoles, string role)
    {
        return userRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
