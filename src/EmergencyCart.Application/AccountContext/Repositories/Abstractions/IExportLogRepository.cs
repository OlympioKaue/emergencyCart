using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IExportLogRepository : IRepository<ExportLog>
{
    Task AddAsync(ExportLog exportLog, CancellationToken cancellationToken);
}