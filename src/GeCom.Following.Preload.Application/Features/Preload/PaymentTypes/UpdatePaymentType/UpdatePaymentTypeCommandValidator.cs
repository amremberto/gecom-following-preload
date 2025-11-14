using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.UpdatePaymentType;

/// <summary>
/// Validator for UpdatePaymentTypeCommand.
/// </summary>
internal sealed class UpdatePaymentTypeCommandValidator : AbstractValidator<UpdatePaymentTypeCommand>
{
    public UpdatePaymentTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("Descripcion is required.")
            .MaximumLength(50)
            .WithMessage("Descripcion must not exceed 50 characters.");
    }
}

