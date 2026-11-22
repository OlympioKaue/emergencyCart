using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public sealed class ExportLogMapping : IEntityTypeConfiguration<ExportLog>
{
    public void Configure(EntityTypeBuilder<ExportLog> builder)
    {
        builder.ToTable("ExportLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Format)
            .HasConversion<int>()
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.EmergencyCartId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.RecordCount)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(x => x.ExportedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.ExportedAt);
    }
}