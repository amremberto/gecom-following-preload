using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderByCuit;

/// <summary>
/// Validator for GetProviderByCuitQuery.
/// </summary>
internal sealed class GetProviderByCuitValidator : AbstractValidator<GetProviderByCuitQuery>
{
    public GetProviderByCuitValidator()
    {
        RuleFor(x => x.Cuit)
            .NotEmpty()
            .WithMessage("CUIT is required.");
    }
}

