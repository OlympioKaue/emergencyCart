using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmergencyCart.Infrastructure.SharedContext.Data.Mappings;

public class EmployeeMapping : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employee");
        builder.HasKey(employee => employee.Id)
            .HasName("PK_Employee");

        builder.OwnsOne(employee => employee.Name, config =>
        {
            config.Property(user => user.FirstName)
            .HasColumnName("FirstName")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100)
            .IsRequired(true);

            config.Property(employee => employee.LastName)
            .HasColumnName("LastName")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100)
            .IsRequired(true);
        });


        builder.OwnsOne(employee => employee.Email, config =>
        {
            config.HasIndex(employee => employee.Address)
            .HasDatabaseName("I_Employee_Email_Address")
            .IsUnique(true);

            config.Property(employee => employee.Address)
            .HasColumnName("Email")
            .HasColumnType("VARCHAR")
            .HasMaxLength(70)
            .IsRequired(true);
        });


        builder.OwnsOne(employee => employee.Password, config =>
        {
            config.Property(employee => employee.Hash)
            .HasColumnName("Hash")
            .HasColumnType("VARCHAR")
            .HasMaxLength(70)
            .IsRequired(true);
        });

        builder.Property(employee => employee.Roles)
            .HasColumnName("Role")
            .HasColumnType("INT")
            .IsRequired(true);

        builder.Property(employee => employee.UserCode)
            .HasColumnName("Code")
            .HasColumnType("VARCHAR")
            .HasMaxLength(30)
            .IsRequired(true);


        builder.Property(employee => employee.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("BIT")
            .IsRequired(true);
    }
}
