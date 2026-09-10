using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal sealed class EmergencyCartRepository(AppDbContext dbContext) : IEmergencyCartRepository
{
    public async Task AddAsync(Domain.AccountContext.Entities.EmergencyCart emergencyCart, CancellationToken cancellationToken)
    {
        await dbContext.EmergencyCarts.AddAsync(emergencyCart, cancellationToken);
    }

    public async Task<Domain.AccountContext.Entities.EmergencyCart?> GetByIdAsync(Guid sectorId, CancellationToken cancellationToken)
    => await dbContext.EmergencyCarts.AsNoTracking().FirstOrDefaultAsync(e => e.SectorId == sectorId, cancellationToken);
}
