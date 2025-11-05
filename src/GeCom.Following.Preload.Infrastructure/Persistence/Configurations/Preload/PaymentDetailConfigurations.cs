using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class PaymentDetailConfigurations : IEntityTypeConfiguration<PaymentDetail>
{
    public void Configure(EntityTypeBuilder<PaymentDetail> builder)
    {
        builder.ToTable("DetalleDePago");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Banco)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("banco");
        builder.Property(e => e.FechaAlta)
            .HasDefaultValueSql("(getdate())")
            .HasColumnName("fechaAlta");
        builder.Property(e => e.IdTipoDePago).HasColumnName("idTipoDePago");
        builder.Property(e => e.ImporteRecibido)
            .HasColumnType("money")
            .HasColumnName("importeRecibido");
        builder.Property(e => e.NamePdf)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("namePdf");
        builder.Property(e => e.NroCheque)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("nroCheque");
        builder.Property(e => e.Vencimiento).HasColumnName("vencimiento");

        builder.HasOne(d => d.IdTipoDePagoNavigation).WithMany(p => p.PaymentDetails)
            .HasForeignKey(d => d.IdTipoDePago)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_DetalleDePago_TipoDePago");
    }
}
