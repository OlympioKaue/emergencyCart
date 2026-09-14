using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.AccountContext.Repositories.Abstractions.Security;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;
using EmergencyCart.Infrastructure.AccountContext.Security.Token;
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
        AddToken(services, configuration);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<IEmergencyCartRepository, EmergencyCartRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();

    }

    private static void AddToken(IServiceCollection services, IConfiguration configuration)
    {
        var expirationTimeMinutes = configuration.GetValue<uint>("Settings:Jwt:ExpiresMinutes");
        var signingKey = configuration.GetValue<string>("Settings:Jwt:SigningKey");

        services.AddScoped<ITokenGenerator>(config => new TokenGenerator(expirationTimeMinutes, signingKey!));
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, m => m.MigrationsAssembly("EmergencyCart.API"));
        });
    }
}
