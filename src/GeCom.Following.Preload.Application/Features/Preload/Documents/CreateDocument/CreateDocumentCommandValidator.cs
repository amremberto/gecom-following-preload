using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.CreateDocument;

/// <summary>
/// Validator for CreateDocumentCommand.
/// </summary>
internal sealed class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.ProveedorCuit)
            .NotEmpty()
            .WithMessage("ProveedorCuit is required.");

        RuleFor(x => x.SociedadCuit)
            .NotEmpty()
            .WithMessage("SociedadCuit is required.");

        RuleFor(x => x.TipoDocId)
            .GreaterThan(0)
            .When(x => x.TipoDocId.HasValue)
            .WithMessage("TipoDocId must be greater than 0 when provided.");

        RuleFor(x => x.MontoBruto)
            .GreaterThan(0)
            .When(x => x.MontoBruto.HasValue)
            .WithMessage("MontoBruto must be greater than 0 when provided.");
    }
}

