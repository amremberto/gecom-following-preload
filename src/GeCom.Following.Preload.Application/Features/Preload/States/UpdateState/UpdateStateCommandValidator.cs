using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.States.UpdateState;

/// <summary>
/// Validator for UpdateStateCommand.
/// </summary>
internal sealed class UpdateStateCommandValidator : AbstractValidator<UpdateStateCommand>
{
    public UpdateStateCommandValidator()
    {
        RuleFor(x => x.EstadoId)
            .GreaterThan(0)
            .WithMessage("EstadoId must be greater than 0.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.");

        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Codigo is required.");
    }
}

