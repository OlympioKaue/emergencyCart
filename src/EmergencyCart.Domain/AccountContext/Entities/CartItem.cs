using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class CartItem : Entity, IAggregateRoots
{
    public const int MaxQuantity = 5;

    private CartItem() : base(Guid.CreateVersion7()) { }

    private CartItem(Guid id, Guid emergencyCartId, Guid itemId, int expectedQuantity) : base(id)
    {
        ExpectedQuantity = expectedQuantity;
        EmergencyCartId = emergencyCartId;
        ItemId = itemId;
    }

    public int ExpectedQuantity { get; private set; }
    public Guid EmergencyCartId { get; }
    public Guid ItemId { get; }
    public EmergencyCart EmergencyCart { get; } = null!;
    public Item Item { get; } = null!;


    public static CartItem Create(Guid emergencyCartId, Guid itemId, int expectedQuantity)
    {
        Validate(expectedQuantity);

        var id = Guid.CreateVersion7();
        return new CartItem(id, emergencyCartId, itemId, expectedQuantity);
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new Exception("Amount must be positive.");

        if (ExpectedQuantity + amount > MaxQuantity)
            throw new Exception($"Quantity limit exceeded. Max {MaxQuantity}, current {ExpectedQuantity}.");

        ExpectedQuantity += amount;
    }

    private static void Validate(int amount)
    {
        if (amount <= 0)
            throw new Exception("Amount must be positive.");

        if (amount > MaxQuantity)
            throw new Exception($"Quantity limit exceeded. Max {MaxQuantity}, current {amount}.");
    }
}
