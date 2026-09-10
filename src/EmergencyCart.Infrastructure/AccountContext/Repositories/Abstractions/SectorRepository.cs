using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal class SectorRepository(AppDbContext dbContext) : ISectorRepository
{
    public async Task AddAsync(Sector sector, CancellationToken cancellationToken)
    {
       await dbContext.AddAsync(sector, cancellationToken);
    }

    public async Task<Sector?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    => await dbContext.Sectors.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<bool> VerifyExistNameSector(string name, CancellationToken cancellationToken) 
        => await dbContext.Sectors
        .AsNoTracking()
        .AnyAsync(active => active.IsActive && 
        active.Names == name, cancellationToken);
}


