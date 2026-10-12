using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.SharedContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace EmergencyCart.Domain.AccountContext.Entities;

public sealed class InspectionItem : Entity
{
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
    public Inspection Inspection { get; } = null!;
    public Item Item { get; } = null!;

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
