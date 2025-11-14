using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.GetSapAccountByCuit;

/// <summary>
/// Validator for the GetSapAccountByCuitQuery.
/// </summary>
internal sealed class GetSapAccountByCuitValidator : AbstractValidator<GetSapAccountByCuitQuery>
{
    public GetSapAccountByCuitValidator()
    {
        RuleFor(x => x.Cuit)
            .NotEmpty()
            .WithMessage("CUIT is required.");
    }
}

