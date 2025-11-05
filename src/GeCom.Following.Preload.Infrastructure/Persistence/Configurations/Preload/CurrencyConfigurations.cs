using GeCom.Following.Preload.Domain.Preloads.Currencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class CurrencyConfigurations : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Monedas");

        builder.HasKey(e => e.Codigo);

        builder.Property(e => e.Codigo)
            .HasMaxLength(4)
            .IsUnicode(false);
        builder.Property(e => e.CodigoAfip)
            .HasMaxLength(3)
            .HasColumnName("CodigoAFIP");
        builder.Property(e => e.Descripcion).IsUnicode(false);
        builder.Property(e => e.MonedaId).ValueGeneratedOnAdd();
    }
}
