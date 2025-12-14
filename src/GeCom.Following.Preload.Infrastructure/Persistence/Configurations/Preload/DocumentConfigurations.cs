using GeCom.Following.Preload.Domain.Preloads.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

/// <summary>
/// Configuration class for the Document entity.
/// </summary>
/// <remarks>
/// This class configures the database mapping, constraints, and relationships
/// for the Document entity in the preload system.
/// </remarks>
public class DocumentConfigurations : IEntityTypeConfiguration<Document>
{
    /// <summary>
    /// Configures the Document entity for the database.
    /// </summary>
    /// <param name="builder">The entity type builder for Document.</param>
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documentos");

        builder.HasKey(e => e.DocId)
            .HasName("PK_Documentos_v2");

        builder.ToTable(tb => tb.HasTrigger("EstadoDocumentosTrigger"));

        builder.Property(e => e.Caecai)
            .HasMaxLength(14)
            .IsUnicode(false)
            .HasColumnName("CAECAI");

        builder.Property(e => e.CodigoDeBarras)
            .IsUnicode(false);

        builder.Property(e => e.FechaBaja)
            .HasColumnType("datetime");

        builder.Property(e => e.FechaCreacion)
            .HasColumnType("datetime");

        builder.Property(e => e.FechaPago)
            .HasColumnType("datetime")
            .HasColumnName("fechaPago");

        builder.Property(e => e.IdDetalleDePago)
            .HasColumnName("idDetalleDePago");

        builder.Property(e => e.IdDocument)
            .HasColumnName("idDocument");

        builder.Property(e => e.Moneda)
            .HasMaxLength(4)
            .IsUnicode(false);

        builder.Property(e => e.MontoBruto)
            .HasColumnType("money");

        builder.Property(e => e.NombreSolicitante)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.NumeroComprobante)
            .HasMaxLength(12)
            .IsUnicode(false);

        builder.Property(e => e.ProveedorCuit)
            .HasMaxLength(12)
            .IsUnicode(false);

        builder.Property(e => e.PuntoDeVenta)
            .HasMaxLength(5)
            .IsUnicode(false);

        builder.Property(e => e.SociedadCuit)
            .HasMaxLength(12)
            .IsUnicode(false);

        builder.Property(e => e.UserCreate)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(e => e.VencimientoCaecai)
            .HasColumnName("VencimientoCAECAI");


        builder.HasOne(d => d.State)
            .WithMany(d => d.Documents)
            .HasForeignKey(d => d.EstadoId)
            .HasConstraintName("FK__Documento__Estad__267ABA7A");

        builder.HasOne(d => d.PaymentDetail)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.IdDetalleDePago)
            .HasConstraintName("FK_Documentos_DetalleDePago");

        builder.HasOne(d => d.Currency)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.Moneda)
            .HasConstraintName("FK__Documento__Moned__276EDEB3");

        builder.HasOne(d => d.Provider)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProveedorCuit)
            .HasConstraintName("FK_Documentos_Proveedores");

        builder.HasOne(d => d.Society)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.SociedadCuit)
            .HasConstraintName("FK__Documento__Socie__2A164134");

        builder.HasOne(d => d.DocumentType)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.TipoDocId)
            .HasConstraintName("FK__Documento__TipoD__2B0A656D");
    }
}
