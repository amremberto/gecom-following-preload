using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.ConfirmPayment;

/// <summary>
/// Validator for ConfirmPaymentCommand.
/// </summary>
internal sealed class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    private const string ChequePaymentMethod = "Cheque o echeq";

    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.");

        When(x => string.Equals(x.PaymentMethod, ChequePaymentMethod, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.NumeroCheque)
                .NotEmpty()
                .WithMessage("Número de cheque is required when payment method is Cheque o echeq.");

            RuleFor(x => x.Banco)
                .NotEmpty()
                .WithMessage("Banco is required when payment method is Cheque o echeq.");

            RuleFor(x => x.Vencimiento)
                .NotNull()
                .WithMessage("Vencimiento is required when payment method is Cheque o echeq.");
        });
    }
}
