using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.States.CreateState;

/// <summary>
/// Validator for CreateStateCommand.
/// </summary>
internal sealed class CreateStateCommandValidator : AbstractValidator<CreateStateCommand>
{
    public CreateStateCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.");

        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Codigo is required.");
    }
}

