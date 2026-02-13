using System.Globalization;
using GeCom.Following.Preload.Application.Abstractions.Pdf;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GeCom.Following.Preload.Infrastructure.Pdf;

/// <summary>
/// QuestPDF-based implementation of <see cref="IPdfDocumentService"/>.
/// </summary>
internal sealed class QuestPdfDocumentService : IPdfDocumentService
{
    private readonly ILogger<QuestPdfDocumentService> _logger;

    static QuestPdfDocumentService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuestPdfDocumentService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public QuestPdfDocumentService(ILogger<QuestPdfDocumentService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateAsync(PdfDocumentRequest document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        _logger.LogDebug("Generating PDF document type {DocumentType}", document.DocumentType);

        Document doc = document.DocumentType == PdfDocumentType.ReciboDePago && document.ReciboDePagoData != null
            ? BuildReciboDePagoDocument(document.ReciboDePagoData)
            : BuildPlaceholderDocument(document);

        byte[] bytes = doc.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private static Document BuildPlaceholderDocument(PdfDocumentRequest request)
    {
        string title = string.IsNullOrWhiteSpace(request.Title) ? "Document" : request.Title;
        string content = string.IsNullOrWhiteSpace(request.Content) ? "No content." : request.Content;

        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(12));

            page.Header()
                .Text(title)
                .SemiBold()
                .FontSize(24)
                .FontColor(Colors.Blue.Medium);

            page.Content()
                .PaddingVertical(1, Unit.Centimetre)
                .Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text(content);
                });

            page.Footer()
                .AlignCenter()
                .Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
        }));
    }

    private static Document BuildReciboDePagoDocument(ReciboDePagoData d)
    {
        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Content().Column(column =>
            {
                column.Spacing(14);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(prov =>
                    {
                        prov.Item().Text("Proveedor").Bold().FontSize(12).Underline();
                        prov.Item().PaddingTop(4).Text(
                            string.IsNullOrWhiteSpace(d.ProveedorNro)
                                ? d.ProveedorCuit
                                : $"{d.ProveedorNro} {d.ProveedorRazonSocial}");
                        prov.Item().Text(string.IsNullOrWhiteSpace(d.ProveedorNro) ? d.ProveedorRazonSocial : d.ProveedorCuit);
                        if (!string.IsNullOrWhiteSpace(d.ProveedorDireccion))
                        {
                            prov.Item().Text(d.ProveedorDireccion);
                        }

                        if (!string.IsNullOrWhiteSpace(d.ProveedorTelefono))
                        {
                            prov.Item().Text("Teléfono: " + d.ProveedorTelefono);
                        }
                    });

                    row.RelativeItem().AlignRight().Column(head =>
                    {
                        head.Item().Text("RECIBO").Bold().FontSize(18);
                        head.Item().Text("Nro: " + (string.IsNullOrWhiteSpace(d.NumeroRecibo) ? "—" : d.NumeroRecibo));
                        head.Item().Text("Fecha: " + d.FechaEmision.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
                        if (!string.IsNullOrWhiteSpace(d.OrdenDePago))
                        {
                            head.Item().PaddingTop(8).Text("Orden de Pago: " + d.OrdenDePago);
                        }
                    });
                });

                column.Item().PaddingTop(12).Padding(8).Background(Colors.Blue.Lighten3).Column(c => c.Item().Text(text =>
                {
                    text.Span("He recibido conforme el pago de: ");
                    text.Span(d.Cliente).Bold();
                }));

                column.Item().Padding(8).Background(Colors.Amber.Lighten3).Column(c => c.Item().Text(text =>
                {
                    text.Span("Por concepto de: ");
                    text.Span(d.Concepto).Bold();
                }));

                column.Item().Padding(8).Background(Colors.Grey.Lighten3).Column(c => c.Item().Text(text =>
                {
                    text.Span("Importe: ");
                    text.Span(d.ImporteRecibido.ToString("N2", CultureInfo.InvariantCulture)).Bold();
                    if (!string.IsNullOrWhiteSpace(d.Moneda))
                    {
                        text.Span(" " + d.Moneda);
                    }
                }));

                column.Item().PaddingTop(16).Column(detalle =>
                {
                    detalle.Item().Text("Detalle de pago").Bold().FontSize(12);
                    detalle.Item().PaddingTop(6).Row(r =>
                    {
                        r.ConstantItem(120).Text(d.EsTransferencia ? "X" : "  ").Bold();
                        r.RelativeItem().Text("Transferencia");
                    });
                    detalle.Item().Row(r =>
                    {
                        r.ConstantItem(120).Text(!d.EsTransferencia ? "X" : "  ").Bold();
                        r.RelativeItem().Text("Cheque o echeq");
                    });
                    if (!d.EsTransferencia)
                    {
                        detalle.Item().PaddingTop(8).Row(r =>
                        {
                            r.ConstantItem(180).Text("Nro de cheque o echeq:");
                            r.RelativeItem().Text(d.NroCheque ?? "—");
                        });
                        detalle.Item().Row(r =>
                        {
                            r.ConstantItem(180).Text("Banco");
                            r.RelativeItem().Text(d.Banco ?? "—");
                        });
                        detalle.Item().Row(r =>
                        {
                            r.ConstantItem(180).Text("Vencimiento");
                            r.RelativeItem().Text(d.Vencimiento?.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) ?? "—");
                        });
                    }
                });

                column.Item().PaddingTop(20).AlignRight().Text("Fecha de alta: " + d.FechaAlta.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)).FontSize(10).FontColor(Colors.Grey.Medium);
            });
        }));
    }
}
