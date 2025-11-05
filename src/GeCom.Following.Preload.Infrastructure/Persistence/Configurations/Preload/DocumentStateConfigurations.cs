using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class DocumentStateConfigurations : IEntityTypeConfiguration<DocumentState>
{
    public void Configure(EntityTypeBuilder<DocumentState> builder)
    {
        builder.ToTable("EstadoDocumento");

        builder.HasKey(e => e.EstDocId);

        builder.ToTable("EstadoDocumento");

        builder.Property(e => e.FechaBaja).HasColumnType("datetime");
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");

        builder.HasOne(d => d.Document).WithMany(p => p.DocumentStates)
            .HasForeignKey(d => d.DocId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_EstadoDocumento_Documentos");

        builder.HasOne(d => d.State).WithMany(p => p.DocumentStates)
            .HasForeignKey(d => d.EstadoId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_EstadoDocumento_Estados");
    }
}
