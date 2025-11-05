using GeCom.Following.Preload.Domain.Preloads.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class StateConfigurations : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("Estados");

        builder.HasKey(e => e.EstadoId);

        builder.Property(e => e.Codigo)
                .HasMaxLength(100)
                .HasDefaultValue("A");
        builder.Property(e => e.Descripcion).IsUnicode(false);
        builder.Property(e => e.FechaBaja).HasColumnType("datetime");
        builder.Property(e => e.FechaCreacion)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");
    }
}
