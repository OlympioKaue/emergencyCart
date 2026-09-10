using EmergencyCart.Application.SharedContext.Repositories.Abstractions;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IEmergencyCartRepository : IRepository<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart>
{
    Task AddAsync(EmergencyCart.Domain.AccountContext.Entities.EmergencyCart emergencyCart, CancellationToken cancellationToken);
    Task<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart?> GetByIdAsync(Guid sectorId, CancellationToken cancellationToken);
}
