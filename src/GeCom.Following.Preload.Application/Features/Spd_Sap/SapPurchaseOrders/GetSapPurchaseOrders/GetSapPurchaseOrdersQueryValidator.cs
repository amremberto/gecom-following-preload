using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrders;

/// <summary>
/// Validator for GetSapPurchaseOrdersQuery.
/// </summary>
internal sealed class GetSapPurchaseOrdersQueryValidator
    : AbstractValidator<GetSapPurchaseOrdersQuery>
{
    public GetSapPurchaseOrdersQueryValidator()
    {
        RuleFor(x => x.UserRoles)
            .NotNull()
            .WithMessage("User roles are required.")
            .NotEmpty()
            .WithMessage("At least one user role is required.");

        // UserEmail is required for Societies role
        When(x => x.UserRoles.Contains("Following.Preload.Societies", StringComparer.OrdinalIgnoreCase),
            () => RuleFor(x => x.UserEmail)
                .NotEmpty()
                .WithMessage("User email is required for users with Societies role.")
                .NotNull()
                .WithMessage("User email cannot be null.")
                .EmailAddress()
                .WithMessage("User email must be a valid email address."));

        // ProviderCuit is required for Providers role
        When(x => x.UserRoles.Contains("Following.Preload.Providers", StringComparer.OrdinalIgnoreCase),
            () => RuleFor(x => x.ProviderCuit)
                .NotEmpty()
                .WithMessage("Provider CUIT is required for users with Providers role.")
                .NotNull()
                .WithMessage("Provider CUIT cannot be null."));
    }
}
