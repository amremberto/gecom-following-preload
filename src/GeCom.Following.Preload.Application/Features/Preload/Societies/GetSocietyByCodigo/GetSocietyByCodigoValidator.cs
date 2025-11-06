using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCodigo;

/// <summary>
/// Validator for GetSocietyByCodigoQuery.
/// </summary>
internal sealed class GetSocietyByCodigoValidator : AbstractValidator<GetSocietyByCodigoQuery>
{
    public GetSocietyByCodigoValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty()
            .WithMessage("Codigo is required.");
    }
}

