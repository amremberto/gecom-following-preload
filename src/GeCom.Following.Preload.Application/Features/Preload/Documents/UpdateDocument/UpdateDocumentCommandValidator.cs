using System.Text.RegularExpressions;
using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocument;

/// <summary>
/// Validator for UpdateDocumentCommand.
/// </summary>
internal sealed class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("UserEmail is required.")
            .EmailAddress()
            .WithMessage("UserEmail must be a valid email address.");

        // Validaciones opcionales para campos que pueden ser actualizados
        RuleFor(x => x.ProveedorCuit)
            .NotEmpty()
            .WithMessage("ProveedorCuit cannot be empty when provided.")
            .When(x => x.ProveedorCuit is not null);

        RuleFor(x => x.SociedadCuit)
            .NotEmpty()
            .WithMessage("SociedadCuit cannot be empty when provided.")
            .When(x => x.SociedadCuit is not null);

        RuleFor(x => x.TipoDocId!.Value)
            .GreaterThan(0)
            .WithMessage("TipoDocId must be greater than 0 when provided.")
            .When(x => x.TipoDocId.HasValue);

        RuleFor(x => x.PuntoDeVenta)
            .NotEmpty()
            .WithMessage("PuntoDeVenta cannot be empty when provided.")
            .When(x => x.PuntoDeVenta is not null)
            .Must(puntoDeVenta => puntoDeVenta is null || Regex.IsMatch(puntoDeVenta, @"^\d{1,5}$"))
            .WithMessage("El punto de venta no debe tener más de 5 dígitos.")
            .When(x => x.PuntoDeVenta is not null);

        RuleFor(x => x.NumeroComprobante)
            .NotEmpty()
            .WithMessage("NumeroComprobante cannot be empty when provided.")
            .When(x => x.NumeroComprobante is not null)
            .Must(numeroComprobante => numeroComprobante is null || Regex.IsMatch(numeroComprobante, @"^\d{1,8}$"))
            .WithMessage("El número de comprobante no debe tener más de 8 dígitos.")
            .When(x => x.NumeroComprobante is not null);

        RuleFor(x => x.Moneda)
            .NotEmpty()
            .WithMessage("Moneda cannot be empty when provided.")
            .When(x => x.Moneda is not null);

        RuleFor(x => x.MontoBruto!.Value)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MontoBruto must be greater than or equal to 0 when provided.")
            .When(x => x.MontoBruto.HasValue);

        RuleFor(x => x.EstadoId!.Value)
            .GreaterThan(0)
            .WithMessage("EstadoId must be greater than 0 when provided.")
            .When(x => x.EstadoId.HasValue);

        // Validación de CAE/CAI: debe tener exactamente 14 dígitos cuando se proporciona
        RuleFor(x => x.Caecai)
            .Must(caecai => caecai is not null && Regex.IsMatch(caecai, @"^\d{14}$"))
            .WithMessage("El nro. CAE/CAI debe tener exactamente 14 dígitos.")
            .When(x => x.Caecai is not null);

        // Validación de VencimientoCaecai: no puede ser anterior a FechaEmisionComprobante
        RuleFor(x => x.VencimientoCaecai)
            .Must((command, vencimiento) => 
                !vencimiento.HasValue || 
                !command.FechaEmisionComprobante.HasValue || 
                vencimiento.Value >= command.FechaEmisionComprobante.Value)
            .WithMessage("La fecha Venc. CAE/CAI no puede ser anterior a la fecha de factura.")
            .When(x => x.VencimientoCaecai.HasValue && x.FechaEmisionComprobante.HasValue);
    }
}
