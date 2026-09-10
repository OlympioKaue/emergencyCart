using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class EmergencyCart : Entity, IAggregateRoots
{
    private readonly List<CartItem> _cartItems;

    private EmergencyCart() : base(Guid.CreateVersion7()) { }

    private EmergencyCart(Guid id, Guid sectorId, string code) : base(id)
    {
        _cartItems = [];
        SectorId = sectorId;
        Code = code;
        IsActive = true;
    }

    public string Code { get; } = string.Empty;
    public bool IsActive { get; } = false;
    public Guid SectorId { get; }
    public Sector Sector { get; } = null!;
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.ToArray();

    public static EmergencyCart Create(Guid sectorId)
    {
        var id = Guid.CreateVersion7();
        var code = GenerateFrom(id);
        return new EmergencyCart(id, sectorId, code);
    }

    private static string GenerateFrom(Guid cartId)
    {
        var shortCode = cartId.ToString("N")[..8].ToUpperInvariant();
        return $"CART-{shortCode}";
    }
}
