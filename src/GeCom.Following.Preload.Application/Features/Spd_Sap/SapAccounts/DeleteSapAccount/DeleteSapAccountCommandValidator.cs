using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.DeleteSapAccount;

/// <summary>
/// Validator for the DeleteSapAccountCommand.
/// </summary>
internal sealed class DeleteSapAccountCommandValidator : AbstractValidator<DeleteSapAccountCommand>
{
    public DeleteSapAccountCommandValidator()
    {
        RuleFor(x => x.Accountnumber)
            .NotEmpty()
            .WithMessage("Account number is required.")
            .MaximumLength(20)
            .WithMessage("Account number must not exceed 20 characters.");
    }
}

