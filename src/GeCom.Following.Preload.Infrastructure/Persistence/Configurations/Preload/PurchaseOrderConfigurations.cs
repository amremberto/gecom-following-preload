using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class PurchaseOrderConfigurations : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("OrdenesCompra");

        builder.HasKey(e => e.Ocid);

        builder.HasIndex(e => new { e.CodigoSociedadFi, e.ProveedorSap, e.NroOc, e.PosicionOc }, "UC_Ordenes");

        builder.Property(e => e.Ocid).HasColumnName("OCId");
        builder.Property(e => e.CantidadAfacturar)
            .HasColumnType("numeric(18, 3)")
            .HasColumnName("CantidadAFacturar");
        builder.Property(e => e.CodigoRecepcion)
            .HasMaxLength(50)
            .IsUnicode(false);
        builder.Property(e => e.CodigoSociedadFi)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasDefaultValue("-1")
            .HasColumnName("codigoSociedadFI");
        builder.Property(e => e.FechaBaja).HasColumnType("datetime");
        builder.Property(e => e.FechaCreacion).HasColumnType("datetime");
        builder.Property(e => e.NroOc)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValue("-1")
            .HasColumnName("NroOC");
        builder.Property(e => e.PosicionOc)
            .HasDefaultValue(-1)
            .HasColumnName("posicionOC");
        builder.Property(e => e.ProveedorSap)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValue("-1")
            .HasColumnName("proveedorSAP");

        builder.HasOne(d => d.Document).WithMany(p => p.PurchaseOrders)
            .HasForeignKey(d => d.DocId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__OrdenesCo__DocId__300424B4");
    }
}
