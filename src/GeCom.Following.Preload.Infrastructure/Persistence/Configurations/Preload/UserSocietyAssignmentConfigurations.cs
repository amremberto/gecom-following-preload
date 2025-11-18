using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Configurations.Preload;

/// <summary>
/// Configuration for UserSocietyAssignment entity.
/// </summary>
public class UserSocietyAssignmentConfigurations : IEntityTypeConfiguration<UserSocietyAssignment>
{
    public void Configure(EntityTypeBuilder<UserSocietyAssignment> builder)
    {
        builder.ToTable("MultiProveedorRelacion");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(e => e.CuitClient)
            .IsRequired()
            .HasMaxLength(12)
            .IsUnicode(false);

        builder.Property(e => e.SociedadFi)
            .IsRequired()
            .HasMaxLength(4)
            .IsUnicode(false);
    }
}

