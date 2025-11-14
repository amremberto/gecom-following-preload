using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.DeletePaymentType;

/// <summary>
/// Validator for DeletePaymentTypeCommand.
/// </summary>
internal sealed class DeletePaymentTypeCommandValidator : AbstractValidator<DeletePaymentTypeCommand>
{
    public DeletePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");
    }
}

