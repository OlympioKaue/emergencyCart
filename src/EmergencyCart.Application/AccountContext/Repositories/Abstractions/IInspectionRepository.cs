using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IInspectionRepository : IRepository<Inspection>
{
    Task AddAsync(Inspection inspection, CancellationToken cancellationToken);
    Task<bool> ExistsInProgressInspectionAsync(Guid emergencyCartId, CancellationToken cancellationToken);
    Task<Inspection?> GetByIdWithItemsAsync(Guid inspectionId, CancellationToken cancellationToken);
    Task<Inspection?> GetByIdWithItemDetailsAsync(Guid inspectionId, CancellationToken cancellationToken);
    Task<Inspection?> GetInProgressByEmergencyCartIdAsync(Guid emergencyCartId, CancellationToken cancellationToken);
    Task<Inspection?> GetLastCompletedByEmergencyCartIdAsync(Guid emergencyCartId, CancellationToken cancellationToken);
    Task<List<Inspection>> GetLastCompletedOfEachCartWithItemDetailsAsync(CancellationToken cancellationToken);
    Task<Inspection?> GetByIdWithReplenishmentsAsync(Guid inspectionId, CancellationToken cancellationToken);

}
