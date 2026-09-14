using EmergencyCart.Application.SharedContext.Repositories.Abstractions;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IEmergencyCartRepository : IRepository<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart>
{
    Task AddAsync(EmergencyCart.Domain.AccountContext.Entities.EmergencyCart emergencyCart, CancellationToken cancellationToken);
    Task<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart?> GetBySectorAsync(Guid sectorId, CancellationToken cancellationToken);
    Task<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart?> GetByIdAsync(Guid emergencyCart, CancellationToken cancellationToken);
    Task<List<EmergencyCart.Domain.AccountContext.Entities.EmergencyCart>> GetAll(CancellationToken cancellationToken);
}
