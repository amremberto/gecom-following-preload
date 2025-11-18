using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.UpdateUserSocietyAssignment;

/// <summary>
/// Validator for UpdateUserSocietyAssignmentCommand.
/// </summary>
internal sealed class UpdateUserSocietyAssignmentCommandValidator : AbstractValidator<UpdateUserSocietyAssignmentCommand>
{
    public UpdateUserSocietyAssignmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(50)
            .WithMessage("Email must not exceed 50 characters.");

        RuleFor(x => x.CuitClient)
            .NotEmpty()
            .WithMessage("CuitClient is required.")
            .MaximumLength(12)
            .WithMessage("CuitClient must not exceed 12 characters.");

        RuleFor(x => x.SociedadFi)
            .NotEmpty()
            .WithMessage("SociedadFi is required.")
            .MaximumLength(4)
            .WithMessage("SociedadFi must not exceed 4 characters.");
    }
}

