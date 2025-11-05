using GeCom.Following.Preload.Domain.Preloads.ActionsRegisters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class ActionsRegisterConfigurations : IEntityTypeConfiguration<ActionsRegister>
{
    public void Configure(EntityTypeBuilder<ActionsRegister> builder)
    {
        builder.ToTable("RegistroAcciones");

        builder.HasKey(e => e.RegId);

        builder.Property(e => e.Accion).IsUnicode(false);
        builder.Property(e => e.Descripcion).IsUnicode(false);
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");
        builder.Property(e => e.NombreCreacion).IsUnicode(false);
        builder.Property(e => e.UsuarioCreacion).IsUnicode(false);

        builder.HasOne(d => d.Document).WithMany(p => p.ActionsRegisters)
            .HasForeignKey(d => d.DocId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__RegistroA__DocId__2C3393D0");
    }
}
