using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public class EmergencyCartMapping : IEntityTypeConfiguration<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart>
{
    public void Configure(EntityTypeBuilder<Domain.AccountContext.Entities.EmergencyCart> builder)
    {
        builder.ToTable("EmergencyCart");
        builder.HasKey(cart => cart.Id)
            .HasName("PK_EmergencyCart");

        builder.Property(cart => cart.Code)
            .HasColumnName("Code")
            .HasColumnType("VARCHAR")
            .HasMaxLength(60)
            .IsRequired(true);

        builder.Property(cart => cart.IsActive)
           .HasColumnName("IsActive")
           .HasColumnType("BIT")
           .IsRequired(true);
    }
}
