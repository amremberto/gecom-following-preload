using GeCom.Following.Preload.Domain.Preloads.Societies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class SocietyConfigurations : IEntityTypeConfiguration<Society>
{
    public void Configure(EntityTypeBuilder<Society> builder)
    {
        builder.ToTable("Sociedades");

        builder.HasKey(e => e.Cuit).HasName("PK__Sociedad__AFA91787F1B5AEDE");

        builder.Property(e => e.Cuit)
            .HasMaxLength(12)
            .IsUnicode(false);
        builder.Property(e => e.Codigo)
            .HasMaxLength(12)
            .IsUnicode(false);
        builder.Property(e => e.Descripcion).HasMaxLength(100);
        builder.Property(e => e.EsPrecarga).HasDefaultValue(false);
        builder.Property(e => e.FechaBaja).HasColumnType("datetime");
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");
        builder.Property(e => e.SocId).ValueGeneratedOnAdd();
    }
}
