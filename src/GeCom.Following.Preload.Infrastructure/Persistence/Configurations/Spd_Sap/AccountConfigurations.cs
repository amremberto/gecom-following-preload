using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Spd_Sap;

public class AccountConfigurations : IEntityTypeConfiguration<SapAccount>
{
    public void Configure(EntityTypeBuilder<SapAccount> builder)
    {
        builder.ToTable("cuenta");

        builder.HasKey(e => e.Accountnumber);

        builder.Property(e => e.Accountnumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("accountnumber");

        builder.Property(e => e.Address1City)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("address1_city");

        builder.Property(e => e.Address1Country)
            .HasMaxLength(80)
            .IsUnicode(false)
            .HasColumnName("address1_country");

        builder.Property(e => e.Address1Line1)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("address1_line1");

        builder.Property(e => e.Address1Postalcode)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("address1_postalcode");

        builder.Property(e => e.Address1Stateorprovince)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("address1_stateorprovince");

        builder.Property(e => e.Cbu)
            .HasMaxLength(22)
            .IsUnicode(false)
            .HasColumnName("cbu");

        builder.Property(e => e.Customertypecode).HasColumnName("customertypecode");

        builder.Property(e => e.Emailaddress1)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("emailaddress1");

        builder.Property(e => e.Fax)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("fax");

        builder.Property(e => e.Name)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("name");

        builder.Property(e => e.NewBloqueado)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasColumnName("new_bloqueado");

        builder.Property(e => e.NewCuit)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("new_cuit");

        builder.Property(e => e.NewGproveedor)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("new_gproveedor");

        builder.Property(e => e.NewIibb)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("new_iibb");

        builder.Property(e => e.NewRubro)
            .HasMaxLength(250)
            .IsUnicode(false)
            .HasColumnName("new_rubro");

        builder.Property(e => e.Telephone1)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("telephone1");
    }
}
