using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Infrastructure.SharedContext.Data;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal class ExportLogRepository(AppDbContext dbContext) : IExportLogRepository
{
    public async Task AddAsync(ExportLog exportLog, CancellationToken cancellationToken)
    {
        await dbContext.ExportLogs.AddAsync(exportLog, cancellationToken);
    }
}
