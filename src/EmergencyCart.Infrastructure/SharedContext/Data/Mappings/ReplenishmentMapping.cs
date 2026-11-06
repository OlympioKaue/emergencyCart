using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public sealed class ReplenishmentMapping : IEntityTypeConfiguration<Replenishment>
{
    public void Configure(EntityTypeBuilder<Replenishment> builder)
    {
        builder.ToTable("Replenishment");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Quantity)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ReplenishedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.InspectionItemId);
    }
}