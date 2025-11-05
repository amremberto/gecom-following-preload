using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Spd_Sap;

public class PurchaseOrderConfigurations : IEntityTypeConfiguration<SapPurchaseOrder>
{
    public void Configure(EntityTypeBuilder<SapPurchaseOrder> builder)
    {
        builder.ToTable("ordenes");

        builder.HasKey(e => e.Idorden);

        builder.HasIndex(e => new { e.Nrodocumento, e.Posicion }, "IX_ordenes").IsUnique();

        builder.Property(e => e.Idorden).HasColumnName("idorden");

        builder.Property(e => e.Almacen)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("almacen");

        builder.Property(e => e.Bloqueado).HasColumnName("bloqueado");

        builder.Property(e => e.Borrado).HasColumnName("borrado");

        builder.Property(e => e.Cantidadentregada)
            .HasColumnType("numeric(18, 3)")
            .HasColumnName("cantidadentregada");

        builder.Property(e => e.Cantidadfacturada)
            .HasColumnType("numeric(18, 3)")
            .HasColumnName("cantidadfacturada");

        builder.Property(e => e.Cantidadpedida)
            .HasColumnType("numeric(18, 3)")
            .HasColumnName("cantidadpedida");

        builder.Property(e => e.Centro)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("centro");

        builder.Property(e => e.Codigosociedadfi)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("codigosociedadfi");

        builder.Property(e => e.Condicionpago)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("condicionpago");

        builder.Property(e => e.Direccionentrega)
            .HasMaxLength(150)
            .IsUnicode(false)
            .HasColumnName("direccionentrega");

        builder.Property(e => e.Dist)
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("dist");

        builder.Property(e => e.Empresa)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("empresa");

        builder.Property(e => e.EntregaFinal).HasColumnName("entrega_final");

        builder.Property(e => e.Fechadocumento)
            .HasColumnType("datetime")
            .HasColumnName("fechadocumento");

        builder.Property(e => e.Importeoriginal)
            .HasColumnType("numeric(18, 2)")
            .HasColumnName("importeoriginal");

        builder.Property(e => e.Liberada).HasColumnName("liberada");

        builder.Property(e => e.Localidad)
            .HasMaxLength(150)
            .IsUnicode(false)
            .HasColumnName("localidad");

        builder.Property(e => e.Material)
            .HasMaxLength(40)
            .IsUnicode(false)
            .HasColumnName("material");

        builder.Property(e => e.Moneda)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("moneda");

        builder.Property(e => e.NetoAnticipo).HasColumnType("numeric(18, 2)");

        builder.Property(e => e.Nrodocumento)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("nrodocumento");

        builder.Property(e => e.Posicion).HasColumnName("posicion");

        builder.Property(e => e.Proveedor)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("proveedor");

        builder.Property(e => e.Textobreve)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("textobreve");

        builder.Property(e => e.Tipo)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("tipo");

        builder.Property(e => e.UnidadCe)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("unidad_ce");

        builder.Property(e => e.UnidadCf)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("unidad_cf");

        builder.Property(e => e.UnidadCp)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("unidad_cp");

        builder.Property(e => e.Usuario)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("usuario");
    }
}
