using GeCom.Following.Preload.Domain.Spd_Sap.SapProviderSocieties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Spd_Sap;

/// <summary>
/// Configuration for SapProviderSocietiy entity.
/// </summary>
public class SapProviderSocietiyConfigurations : IEntityTypeConfiguration<SapProviderSocietiy>
{
    public void Configure(EntityTypeBuilder<SapProviderSocietiy> builder)
    {
        builder.ToTable("Proveedorsociedad");

        // Add composite primary key with Proveedor and Sociedadfi
        builder.HasKey(e => new { e.Proveedor, e.Sociedadfi });

        builder.Property(e => e.Proveedor)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("Proveedor");

        builder.Property(e => e.Sociedadfi)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("Sociedadfi");

        builder.Property(e => e.CondicionPago)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("CondicionPago");

        builder.Property(e => e.Viapago)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("Viapago");

        builder.Property(e => e.Bloqueado)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasColumnName("Bloqueado");

        builder.Property(e => e.CBU)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("CBU");

        builder.Property(e => e.Codigo)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("Codigo");
    }
}
