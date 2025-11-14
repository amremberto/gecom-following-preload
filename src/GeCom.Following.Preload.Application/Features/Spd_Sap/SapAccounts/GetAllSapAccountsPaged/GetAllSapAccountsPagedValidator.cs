using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetAllSapAccountsPaged;

/// <summary>
/// Validator for the GetAllSapAccountsPagedQuery.
/// </summary>
internal sealed class GetAllSapAccountsPagedValidator : AbstractValidator<GetAllSapAccountsPagedQuery>
{
    public GetAllSapAccountsPagedValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(200)
            .WithMessage("Page size must not exceed 200.");
    }
}

