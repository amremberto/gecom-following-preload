using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentDetails.CreatePaymentDetail;

/// <summary>
/// Validator for CreatePaymentDetailCommand.
/// </summary>
internal sealed class CreatePaymentDetailCommandValidator : AbstractValidator<CreatePaymentDetailCommand>
{
    public CreatePaymentDetailCommandValidator()
    {
        RuleFor(x => x.IdTipoDePago)
            .GreaterThan(0)
            .WithMessage("IdTipoDePago must be greater than 0.");

        RuleFor(x => x.NroCheque)
            .NotEmpty()
            .WithMessage("NroCheque is required.")
            .MaximumLength(50)
            .WithMessage("NroCheque must not exceed 50 characters.");

        RuleFor(x => x.Banco)
            .NotEmpty()
            .WithMessage("Banco is required.")
            .MaximumLength(50)
            .WithMessage("Banco must not exceed 50 characters.");

        RuleFor(x => x.NamePdf)
            .NotEmpty()
            .WithMessage("NamePdf is required.")
            .MaximumLength(50)
            .WithMessage("NamePdf must not exceed 50 characters.");

        RuleFor(x => x.ImporteRecibido)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ImporteRecibido must be greater than or equal to 0.");
    }
}
