using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.States.DeleteState;

/// <summary>
/// Validator for DeleteStateCommand.
/// </summary>
internal sealed class DeleteStateCommandValidator : AbstractValidator<DeleteStateCommand>
{
    public DeleteStateCommandValidator()
    {
        RuleFor(x => x.EstadoId)
            .GreaterThan(0)
            .WithMessage("EstadoId must be greater than 0.");
    }
}

