using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.CreatePaymentType;

/// <summary>
/// Validator for CreatePaymentTypeCommand.
/// </summary>
internal sealed class CreatePaymentTypeCommandValidator : AbstractValidator<CreatePaymentTypeCommand>
{
    public CreatePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.")
            .MaximumLength(50)
            .WithMessage("Descripcion must not exceed 50 characters.");
    }
}

