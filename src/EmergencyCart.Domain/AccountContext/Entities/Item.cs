using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.AccountContext.ValueObjects;
using EmergencyCart.Domain.SharedContext.AggregateRoots.Abstractions;
using EmergencyCart.Domain.SharedContext.Entities;
using System.Text.RegularExpressions;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed partial class Item : Entity, IAggregateRoots
{
    public const string Pattern = @"^(?=.*\s)[\p{L}0-9\s%\(\)\/,]+$";
    private readonly List<CartItem> _cartItems;

    private Item() : base(Guid.CreateVersion7()) { }

    private Item(Guid id, string name, ItemCategory category) : base(id)
    {
        Names = name;
        Category = category;
        IsActive = true;
        _cartItems = [];
    }

    public string Names { get; } = string.Empty;
    public bool IsActive { get; } = false;
    public ItemCategory Category { get; }
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.ToArray();

    private static void Validate(string name, ItemCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.");

        if (name.Length < 5 || name.Length > 60)
            throw new ArgumentException("Name must be between 5 and 60 characters long.");

        if (!NameRegex().IsMatch(name))
            throw new ArgumentException("Name contains invalid characters.");

        if (!Enum.IsDefined(typeof(ItemCategory), category))
            throw new ArgumentException("Invalid category.");
    }

    public static Item Create(string name, ItemCategory category)
    {
        Validate(name, category);

        var formattedName = Name.FormatName(name);

        var id = Guid.CreateVersion7();
        return new Item(id, formattedName, category);
    }

    [GeneratedRegex(Pattern)]
    public static partial Regex NameRegex();
}
