using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class DocumentTypeConfigurations : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("TiposDocumento");

        builder.HasKey(e => e.TipoDocId);

        builder.Property(e => e.Codigo)
                .HasMaxLength(4)
                .IsUnicode(false);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(90)
            .IsUnicode(false);

        builder.Property(e => e.DescripcionLarga)
            .HasMaxLength(90)
            .IsUnicode(false);

        builder.Property(e => e.FechaBaja)
            .HasColumnType("datetime");

        builder.Property(e => e.FechaCreacion)
            .HasColumnType("datetime");

        builder.Property(e => e.IsFec)
            .HasColumnName("IsFEC");

        builder.Property(e => e.IsNotaDebitoCredito)
            .HasColumnName("IsNotaDebitoCredito");

        builder.Property(e => e.Letra)
            .HasMaxLength(1)
            .IsUnicode(false);
    }
}
