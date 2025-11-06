using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyById;

/// <summary>
/// Validator for GetSocietyByIdQuery.
/// </summary>
internal sealed class GetSocietyByIdValidator : AbstractValidator<GetSocietyByIdQuery>
{
    public GetSocietyByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than zero.");
    }
}

