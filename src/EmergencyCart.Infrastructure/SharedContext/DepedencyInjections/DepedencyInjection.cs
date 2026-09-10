using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmergencyCart.Infrastructure.SharedContext.DepedencyInjections;

public static class DepedencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddRepositories(services);
        AddDatabase(services, configuration);
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<IEmergencyCartRepository, EmergencyCartRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();

    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, m => m.MigrationsAssembly("EmergencyCart.API"));
        });
    }
}
