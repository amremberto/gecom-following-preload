using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.DeleteUserSocietyAssignment;

/// <summary>
/// Validator for DeleteUserSocietyAssignmentCommand.
/// </summary>
internal sealed class DeleteUserSocietyAssignmentCommandValidator : AbstractValidator<DeleteUserSocietyAssignmentCommand>
{
    public DeleteUserSocietyAssignmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");
    }
}

