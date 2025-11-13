using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetStateById;

/// <summary>
/// Validator for GetStateByIdQuery.
/// </summary>
internal sealed class GetStateByIdQueryValidator : AbstractValidator<GetStateByIdQuery>
{
    public GetStateByIdQueryValidator()
    {
        RuleFor(x => x.EstadoId)
            .GreaterThan(0)
            .WithMessage("EstadoId must be greater than 0.");
    }
}

