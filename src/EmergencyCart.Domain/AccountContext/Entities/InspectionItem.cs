using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class InspectionItem : Entity
{
    public const int MaxNoteLength = 200;

    private readonly List<Replenishment> _replenishments = [];
    private InspectionItem() : base(Guid.CreateVersion7()) { }

    private InspectionItem(Guid id, Guid inspectionId, Guid itemId, int expectedQuantity) : base(id)
    {
        InspectionId = inspectionId;
        ItemId = itemId;
        ExpectedQuantity = expectedQuantity;
    }

    public int ExpectedQuantity { get; }
    public int? FoundQuantity { get; private set; }
    public int? Difference => FoundQuantity is null ? null : FoundQuantity.Value - ExpectedQuantity;
    public int MissingQuantity => Difference is < 0 ? -Difference.Value : 0;
    public Guid InspectionId { get; }
    public Guid ItemId { get; }
    public string? Note { get; private set; } = string.Empty;
    public Inspection Inspection { get; } = null!;
    public Item Item { get; } = null!;
    public IReadOnlyCollection<Replenishment> Replenishments => _replenishments.ToArray();
    public int ReplenishedQuantity => _replenishments.Sum(x => x.Quantity);
    public int PendingQuantity => Math.Max(MissingQuantity - ReplenishedQuantity, 0);

    internal static InspectionItem Create(Guid inspectionId, Guid itemId, int expectedQuantity)
    {
        Validate(inspectionId, itemId, expectedQuantity);
        return new InspectionItem(Guid.CreateVersion7(), inspectionId, itemId, expectedQuantity);
    }

    internal void RegisterFoundQuantity(InspectionItem inspectionItem, int foundQuantity)
    {
        if (foundQuantity < 0 || foundQuantity > CartItem.MaxQuantity)
            throw new ValidationException("Unexpected error: The quantity does not meet system requirements.");

        FoundQuantity = foundQuantity;
    }

    internal void RegisterReplenishment(Guid employeeId, int quantity)
    {
        if (Comparison != ComparisonStatus.Missing)
            throw new ValidationException("Unexpected error: Only missing items can be replenished.");

        if (quantity <= 0 || quantity > PendingQuantity)
            throw new ValidationException("Unexpected error: The quantity does not meet system requirements.");

        _replenishments.Add(Replenishment.Create(Id, employeeId, quantity));
    }

    internal void RegisterNote(string? note)
    {
        if (Comparison != ComparisonStatus.Missing)
            throw new ValidationException("Unexpected error: Only missing items can have a note.");

        var text = note?.Trim();
        if (text is { Length: > MaxNoteLength })
            throw new ValidationException("Unexpected error: The note exceeds the maximum length.");

        Note = string.IsNullOrEmpty(text) ? null : text;
    }

    public ComparisonStatus Comparison => Difference switch
    {
        null => ComparisonStatus.NotChecked,
        0 => ComparisonStatus.Matched,
        < 0 => ComparisonStatus.Missing,
        _ => ComparisonStatus.Surplus,
    };

    private static void Validate(Guid inspectionId, Guid itemId, int expectedQuantity)
    {
        if (inspectionId == Guid.Empty)
            throw new ValidationException("Unexpected error: Invalid inspection ID.");

        if (itemId == Guid.Empty)
            throw new ValidationException("Unexpected error: Invalid item ID.");

        if (expectedQuantity <= 0 || expectedQuantity > CartItem.MaxQuantity)
            throw new ValidationException("Unexpected error: The quantity does not meet system requirements.");
    }
}
