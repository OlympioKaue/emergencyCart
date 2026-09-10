using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public class CarItemMapping : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        #region Table
        builder.ToTable("CartItem");
        builder.HasKey(cartItem => cartItem.Id)
            .HasName("PK_CartItem");
        #endregion

        #region Index
        builder.HasIndex(c => new {c.EmergencyCartId, c.ItemId })
            .IsUnique()
            .HasDatabaseName("IX_CartItem_EmergencyCartId_ItemId");
        #endregion

        #region Column
        builder.Property(car => car.ExpectedQuantity)
            .HasColumnName("ExpectedQuantity")
            .HasColumnType("INT")
            .IsRequired(true);

        builder.HasOne(cart => cart.EmergencyCart)
            .WithMany(car => car.CartItems)
            .HasForeignKey(car => car.EmergencyCartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cart => cart.Item)
            .WithMany(car => car.CartItems)
            .HasForeignKey(car => car.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
        #endregion
    }
}
