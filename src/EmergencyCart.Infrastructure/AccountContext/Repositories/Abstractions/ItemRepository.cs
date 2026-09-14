using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal sealed class ItemRepository(AppDbContext dbContext) : IItemRepository
{
    public async Task AddAsync(Item item, CancellationToken cancellationToken)
    {
        await dbContext.Items.AddAsync(item, cancellationToken);
    }

    public void Update(Item item, CancellationToken cancellationToken)
    {
        dbContext.Items.Update(item);
    }

    public async Task<bool> ExistsItemAsync(string name, ItemCategory category, CancellationToken cancellationToken)
    => await dbContext.Items.AsNoTracking().AnyAsync(i => i.Names == name && i.Category == category, cancellationToken);

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    => await dbContext.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id && i.IsActive, cancellationToken);

    public async Task<List<Item>> GetAllAsync(CancellationToken cancellationToken)
    => await dbContext.Items.AsNoTracking().ToListAsync(cancellationToken);
}

