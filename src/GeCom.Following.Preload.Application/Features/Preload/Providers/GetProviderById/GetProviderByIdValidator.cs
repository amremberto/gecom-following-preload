using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Providers.GetProviderById;

/// <summary>
/// Validator for GetProviderByIdQuery.
/// </summary>
internal sealed class GetProviderByIdValidator : AbstractValidator<GetProviderByIdQuery>
{
    public GetProviderByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0.");
    }
}

