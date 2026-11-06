using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal sealed class InspectionRepository(AppDbContext dbContext) : IInspectionRepository
{
    public async Task AddAsync(Inspection inspection, CancellationToken cancellationToken)
    {
        await dbContext.Inspections.AddAsync(inspection, cancellationToken);
    }

    public Task<bool> ExistsInProgressInspectionAsync(Guid emergencyCartId, CancellationToken cancellationToken)
    => dbContext.Inspections.AnyAsync(i => i.EmergencyCartId == emergencyCartId && i.Status == InspectionStatus.InProgress, cancellationToken);

    public Task<Inspection?> GetByIdWithItemDetailsAsync(Guid inspectionId, CancellationToken cancellationToken)
    => dbContext.Inspections
        .AsNoTracking()
        .Include(x => x.InspectionItems)
          .ThenInclude(x => x.Item)
        .Include(x => x.InspectionItems)
          .ThenInclude(x => x.Replenishments)
        .FirstOrDefaultAsync(i => i.Id == inspectionId, cancellationToken);

    public async Task<Inspection?> GetByIdWithItemsAsync(Guid inspectionId, CancellationToken cancellationToken)
    => await dbContext.Inspections
        .Include(i => i.InspectionItems)
        .FirstOrDefaultAsync(i => i.Id == inspectionId, cancellationToken);

    public async Task<Inspection?> GetByIdWithReplenishmentsAsync(Guid inspectionId, CancellationToken cancellationToken)
   => await dbContext.Inspections
        .Include(i => i.InspectionItems)
        .ThenInclude(x => x.Replenishments)
        .FirstOrDefaultAsync(i => i.Id == inspectionId, cancellationToken);

    public async Task<Inspection?> GetInProgressByEmergencyCartIdAsync(Guid emergencyCartId, CancellationToken cancellationToken)
    => await dbContext.Inspections
        .AsNoTracking()
        .FirstOrDefaultAsync(
            x => x.EmergencyCartId == emergencyCartId && x.Status == InspectionStatus.InProgress,
            cancellationToken);

    public Task<Inspection?> GetLastCompletedByEmergencyCartIdAsync(Guid emergencyCartId, CancellationToken cancellationToken)
    => dbContext.
        Inspections
        .AsNoTracking()
        .Where(x => x.EmergencyCartId == emergencyCartId && x.Status == InspectionStatus.Completed)
        .OrderByDescending(x => x.FinishedAt)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Inspection>> GetLastCompletedOfEachCartWithItemDetailsAsync(CancellationToken cancellationToken)
   => await dbContext.Inspections
            .AsNoTracking()
            .Where(x => x.Status == InspectionStatus.Completed
            && x.FinishedAt != null
            && x.FinishedAt == dbContext.Inspections
               .Where(o => o.EmergencyCartId == x.EmergencyCartId && o.Status == InspectionStatus.Completed)
               .Max(o => o.FinishedAt))
            .Include(i => i.EmergencyCart)
              .ThenInclude(c => c.Sector)
             .Include(x => x.InspectionItems)
             .ThenInclude(x => x.Replenishments)
               .Include(x => x.InspectionItems)
                 .ThenInclude(x => x.Item)
             .ToListAsync(cancellationToken);

}
