using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;

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
        return new CartItem(Guid.CreateVersion7(), emergencyCartId, itemId, expectedQuantity);
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new ValidationException("Unexpected error: The quantity does not meet system requirements.");

        if (ExpectedQuantity + amount > MaxQuantity)
            throw new ValidationException($"Unexpected error: The quantity does not meet system requirements.");

        ExpectedQuantity += amount;
    }

    private static void Validate(int amount)
    {
        if (amount <= 0)
            throw new ValidationException("Unexpected error: The quantity does not meet system requirements.");

        if (amount > MaxQuantity)
            throw new Exception($"Unexpected error: The quantity does not meet system requirements.");
    }
}
