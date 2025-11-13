using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetStateByCode;

/// <summary>
/// Validator for GetStateByCodeQuery.
/// </summary>
internal sealed class GetStateByCodeQueryValidator : AbstractValidator<GetStateByCodeQuery>
{
    public GetStateByCodeQueryValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Codigo is required.");
    }
}

