using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public class ItemMapping : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Item");
        builder.HasKey(item => item.Id)
            .HasName("PK_Item");
    
        builder.Property(item => item.Names)
            .HasColumnName("Name")
            .HasColumnType("VARCHAR")
            .HasMaxLength(60)
            .IsRequired(true);

        builder.Property(item => item.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("BIT")
            .IsRequired(true);

        builder.Property(item => item.Category)
          .HasColumnName("Category")
          .HasColumnType("INT")
          .IsRequired(true);
    }
}
