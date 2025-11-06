using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCuit;

/// <summary>
/// Validator for GetSocietyByCuitQuery.
/// </summary>
internal sealed class GetSocietyByCuitValidator : AbstractValidator<GetSocietyByCuitQuery>
{
    public GetSocietyByCuitValidator()
    {
        RuleFor(x => x.Cuit)
            .NotEmpty()
            .WithMessage("CUIT is required.");
    }
}

