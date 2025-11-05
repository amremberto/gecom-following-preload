using GeCom.Following.Preload.Domain.Preloads.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class ProviderConfigurations : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Proveedores");

        builder.HasKey(e => e.Cuit);

        builder.Property(e => e.Cuit)
            .HasMaxLength(12)
            .IsUnicode(false);
        builder.Property(e => e.FechaBaja).HasColumnType("datetime");
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");
        builder.Property(e => e.Mail)
            .HasMaxLength(100)
            .IsUnicode(false);
        builder.Property(e => e.ProvId).ValueGeneratedOnAdd();
        builder.Property(e => e.RazonSocial)
            .HasMaxLength(200)
            .IsUnicode(false);
    }
}
