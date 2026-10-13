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
        .FirstOrDefaultAsync(i => i.Id == inspectionId, cancellationToken);

    public async Task<Inspection?> GetByIdWithItemsAsync(Guid inspectionId, CancellationToken cancellationToken)
    => await dbContext.Inspections
        .Include(i => i.InspectionItems)
        .FirstOrDefaultAsync(i => i.Id == inspectionId, cancellationToken);

    public async Task<Inspection?> GetInProgressByEmergencyCartIdAsync(Guid emergencyCartId, CancellationToken cancellationToken)
    => await dbContext.Inspections
        .AsNoTracking()
        .FirstOrDefaultAsync(
            x => x.EmergencyCartId == emergencyCartId && x.Status == InspectionStatus.InProgress,
            cancellationToken);
}
