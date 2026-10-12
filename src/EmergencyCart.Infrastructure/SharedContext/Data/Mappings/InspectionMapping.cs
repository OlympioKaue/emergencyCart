using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public class InspectionMapping : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspection");
        builder.HasKey(i => i.Id)
            .HasName("PK_Inspection");

        builder.Property(i => i.Status)
            .HasColumnName("Status")
            .HasColumnType("int")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.StartedAt)
            .HasColumnName("StartedAt")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(i => i.FinishedAt)
            .HasColumnName("FinishedAt")
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.EmergencyCartId)
            .HasFilter("[Status] = 1")
            .IsUnique();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(i => i.EmergencyCart)
            .WithMany()
            .HasForeignKey(i => i.EmergencyCartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.InspectionItems)
            .WithOne(x => x.Inspection)
            .HasForeignKey(x => x.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.InspectionItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
