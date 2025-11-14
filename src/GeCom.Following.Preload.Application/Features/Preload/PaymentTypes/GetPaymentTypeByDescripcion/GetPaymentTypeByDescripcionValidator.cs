using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeByDescripcion;

/// <summary>
/// Validator for GetPaymentTypeByDescripcionQuery.
/// </summary>
internal sealed class GetPaymentTypeByDescripcionValidator : AbstractValidator<GetPaymentTypeByDescripcionQuery>
{
    public GetPaymentTypeByDescripcionValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.");
    }
}

