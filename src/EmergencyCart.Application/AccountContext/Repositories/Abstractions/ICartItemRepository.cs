using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;

namespace EmergencyCart.Application.AccountContext.Repositories.Abstractions;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task<CartItem?> GetCartItemAsync(Guid emergencyCartId, Guid itemId, CancellationToken cancellationToken);
    Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId, CancellationToken cancellationToken);
    void Update(CartItem cartItem);
    Task AddAsync(CartItem cartItem, CancellationToken cancellationToken);
}
