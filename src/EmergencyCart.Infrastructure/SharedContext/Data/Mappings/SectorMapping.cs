using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public class SectorMapping : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sector");
        builder.HasKey(sector => sector.Id)
            .HasName("PK_Sector");

  
        builder.Property(sector => sector.Names)
            .HasColumnName("Name")
            .HasColumnType("VARCHAR")
            .HasMaxLength(60)
            .IsRequired(true);

        builder.Property(sector => sector.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("BIT")
            .IsRequired(true);

        builder.Property(cart => cart.Code)
           .HasColumnName("Code")
           .HasColumnType("VARCHAR")
           .HasMaxLength(60)
           .IsRequired(true);


        builder.HasMany(sector => sector.EmergencyCarts)
            .WithOne(cart => cart.Sector)
            .HasForeignKey(cart => cart.SectorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
 