using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public sealed class InspectionItemMapping : IEntityTypeConfiguration<InspectionItem>
{
    public void Configure(EntityTypeBuilder<InspectionItem> builder)
    {
        builder.ToTable("InspectionItem");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExpectedQuantity)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.FoundQuantity)
            .HasColumnType("int");

        builder.Property(x => x.Note)
            .HasColumnType("NVARCHAR")
           .HasMaxLength(InspectionItem.MaxNoteLength);

        builder.HasOne(x => x.Item)
               .WithMany()
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Replenishments)
            .WithOne(x => x.InspectionItem)
            .HasForeignKey(x => x.InspectionItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Replenishments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.Comparison);
        builder.Ignore(x => x.Difference);
        builder.Ignore(x => x.MissingQuantity);
        builder.Ignore(x => x.ReplenishedQuantity);
        builder.Ignore(x => x.PendingQuantity);

        builder.HasIndex(x => new { x.InspectionId, x.ItemId }).IsUnique();
    }
}
