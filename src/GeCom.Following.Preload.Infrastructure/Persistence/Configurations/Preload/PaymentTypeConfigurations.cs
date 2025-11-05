using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

public class PaymentTypeConfigurations : IEntityTypeConfiguration<PaymentType>
{
    public void Configure(EntityTypeBuilder<PaymentType> builder)
    {
        builder.ToTable("TipoDePago");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("descripcion");
    }
}
