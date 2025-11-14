using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByAccountNumber;

/// <summary>
/// Validator for the GetSapAccountByAccountNumberQuery.
/// </summary>
internal sealed class GetSapAccountByAccountNumberValidator : AbstractValidator<GetSapAccountByAccountNumberQuery>
{
    public GetSapAccountByAccountNumberValidator()
    {
        RuleFor(x => x.Accountnumber)
            .NotEmpty()
            .WithMessage("Account number is required.")
            .MaximumLength(20)
            .WithMessage("Account number must not exceed 20 characters.");
    }
}

