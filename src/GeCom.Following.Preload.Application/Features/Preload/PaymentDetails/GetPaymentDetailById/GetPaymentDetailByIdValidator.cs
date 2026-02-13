using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentDetails.GetPaymentDetailById;

/// <summary>
/// Validator for GetPaymentDetailByIdQuery.
/// </summary>
internal sealed class GetPaymentDetailByIdValidator : AbstractValidator<GetPaymentDetailByIdQuery>
{
    public GetPaymentDetailByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");
    }
}
