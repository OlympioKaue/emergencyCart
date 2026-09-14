using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface ISectorRepository : IRepository<Sector>
{
    Task<bool> VerifyExistNameSector(string name, CancellationToken cancellationToken);
    Task<Sector?> GetByEmergencyCartAsync(string sectorCode, CancellationToken cancellationToken);
    Task AddAsync(Sector sector, CancellationToken cancellationToken);
    Task<List<Sector>> GetAllAsync(CancellationToken cancellationToken);
}
