using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Infrastructure.SharedContext.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyCart.Infrastructure.AccountContext.Repositories.Abstractions;

internal sealed class CartItemRepository(AppDbContext dbContext) : ICartItemRepository
{
    public async Task AddAsync(CartItem cartItem, CancellationToken cancellationToken)
    {
        await dbContext.CartItems.AddAsync(cartItem, cancellationToken);
    }

    public async Task<List<CartItem>> GetAllAsync(CancellationToken cancellationToken)
     => await dbContext.CartItems.Include(i => i.Item).AsNoTracking().ToListAsync();


    public async Task<CartItem?> GetCartItemAsync(Guid emergencyCartId, Guid itemId, CancellationToken cancellationToken)
    => await dbContext.CartItems.FirstOrDefaultAsync(c => c.EmergencyCartId == emergencyCartId && c.ItemId == itemId, cancellationToken);

    public async Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId, CancellationToken cancellationToken)
    => await dbContext.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId, cancellationToken);

    public async Task<List<CartItem>> GetCartItemsByCartIdAsync(Guid emergencyCart, CancellationToken cancellationToken)
    => await dbContext.CartItems.Include(i => i.Item).AsNoTracking().Where(i => i.EmergencyCartId == emergencyCart).ToListAsync(cancellationToken);

    public void Update(CartItem cartItem)
    {
        dbContext.CartItems.Update(cartItem);
    }
}
