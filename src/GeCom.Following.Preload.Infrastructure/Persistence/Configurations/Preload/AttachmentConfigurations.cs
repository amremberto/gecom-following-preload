using GeCom.Following.Preload.Domain.Preloads.Attachments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class AttachmentConfigurations : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Adjuntos");

        builder.HasKey(e => e.AdjuntoId).HasName("PK__Adjuntos__2ECBD540E38F6223");

        builder.Property(e => e.FechaBorrado).HasColumnType("datetime");
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");
        builder.Property(e => e.FechaModificacion).HasColumnType("datetime");
        builder.Property(e => e.Path).IsUnicode(false);

        builder.HasOne(d => d.Doc).WithMany(p => p.Attachments)
            .HasForeignKey(d => d.DocId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Adjuntos__DocId__25869641");
    }
}
