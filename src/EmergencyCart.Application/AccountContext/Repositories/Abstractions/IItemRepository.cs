using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface IItemRepository : IRepository<Item>
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsItemAsync(string name, ItemCategory category, CancellationToken cancellationToken);
    Task AddAsync(Item item, CancellationToken cancellationToken);
    void Update(Item item, CancellationToken cancellationToken);
}
