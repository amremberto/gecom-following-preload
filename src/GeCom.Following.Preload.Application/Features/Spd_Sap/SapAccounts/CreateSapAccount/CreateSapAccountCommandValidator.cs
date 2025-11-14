using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.CreateSapAccount;

/// <summary>
/// Validator for the CreateSapAccountCommand.
/// </summary>
internal sealed class CreateSapAccountCommandValidator : AbstractValidator<CreateSapAccountCommand>
{
    public CreateSapAccountCommandValidator()
    {
        RuleFor(x => x.Accountnumber)
            .NotEmpty()
            .WithMessage("Account number is required.")
            .MaximumLength(20)
            .WithMessage("Account number must not exceed 20 characters.");

        RuleFor(x => x.Name)
            .MaximumLength(250)
            .WithMessage("Name must not exceed 250 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Address1City)
            .MaximumLength(250)
            .WithMessage("City must not exceed 250 characters.")
            .When(x => x.Address1City is not null);

        RuleFor(x => x.Address1Stateorprovince)
            .MaximumLength(250)
            .WithMessage("State or province must not exceed 250 characters.")
            .When(x => x.Address1Stateorprovince is not null);

        RuleFor(x => x.Address1Postalcode)
            .MaximumLength(50)
            .WithMessage("Postal code must not exceed 50 characters.")
            .When(x => x.Address1Postalcode is not null);

        RuleFor(x => x.Address1Line1)
            .MaximumLength(250)
            .WithMessage("Address line 1 must not exceed 250 characters.")
            .When(x => x.Address1Line1 is not null);

        RuleFor(x => x.Telephone1)
            .MaximumLength(50)
            .WithMessage("Telephone must not exceed 50 characters.")
            .When(x => x.Telephone1 is not null);

        RuleFor(x => x.Fax)
            .MaximumLength(50)
            .WithMessage("Fax must not exceed 50 characters.")
            .When(x => x.Fax is not null);

        RuleFor(x => x.Address1Country)
            .MaximumLength(80)
            .WithMessage("Country must not exceed 80 characters.")
            .When(x => x.Address1Country is not null);

        RuleFor(x => x.Emailaddress1)
            .MaximumLength(250)
            .WithMessage("Email must not exceed 250 characters.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .When(x => x.Emailaddress1 is not null);

        RuleFor(x => x.Cbu)
            .MaximumLength(22)
            .WithMessage("CBU must not exceed 22 characters.")
            .When(x => x.Cbu is not null);
    }
}

