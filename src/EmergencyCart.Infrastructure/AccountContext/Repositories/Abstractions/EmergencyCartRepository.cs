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

    public async Task<List<Domain.AccountContext.Entities.EmergencyCart>> GetAll(CancellationToken cancellationToken)
    => await dbContext.EmergencyCarts
        .Include(s => s.Sector)
        .Include(x => x.CartItems)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    public async Task<Domain.AccountContext.Entities.EmergencyCart?> GetByIdAsync(Guid emergencyCart, CancellationToken cancellationToken)
    => await dbContext.EmergencyCarts
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Id == emergencyCart, cancellationToken);

    public async Task<Domain.AccountContext.Entities.EmergencyCart?> GetByIdWithItemsAsync(Guid emergencyCartId, CancellationToken cancellationToken)
    => await dbContext.EmergencyCarts
        .Include(x => x.CartItems)
        .FirstOrDefaultAsync(e => e.Id == emergencyCartId, cancellationToken);

    public async Task<Domain.AccountContext.Entities.EmergencyCart?> GetBySectorAsync(Guid sectorId, CancellationToken cancellationToken)
    => await dbContext.EmergencyCarts
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.SectorId == sectorId, cancellationToken);
}
