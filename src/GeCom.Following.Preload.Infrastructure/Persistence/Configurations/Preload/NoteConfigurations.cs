using GeCom.Following.Preload.Domain.Preloads.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class NoteConfigurations : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notas");

        builder.ToTable(tb => tb.HasTrigger("AddNoteMonitoresTrigger"));

        builder.HasKey(e => e.NotaId);

        builder.Property(e => e.Descripcion).IsUnicode(false);
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");
        builder.Property(e => e.UsuarioCreacion)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.HasOne(d => d.Document).WithMany(p => p.Notes)
            .HasForeignKey(d => d.DocId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Notas__DocId__2A4B4B5E");
    }
}
