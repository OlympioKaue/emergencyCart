using EmergencyCart.Domain.AccountContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.SharedContext.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContext) : base(dbContext) { }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Sector> Sectors { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart> EmergencyCarts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Inspection> Inspections { get; set; } = null!;
    public DbSet<InspectionItem> InspectionItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.HasSequence<int>("EmergencyCartCodeSequence", schema: "dbo")
           .StartsAt(1)
           .IncrementsBy(1);
    }
}



