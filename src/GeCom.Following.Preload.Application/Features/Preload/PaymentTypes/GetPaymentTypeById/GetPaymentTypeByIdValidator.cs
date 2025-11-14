using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeById;

/// <summary>
/// Validator for GetPaymentTypeByIdQuery.
/// </summary>
internal sealed class GetPaymentTypeByIdValidator : AbstractValidator<GetPaymentTypeByIdQuery>
{
    public GetPaymentTypeByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");
    }
}

