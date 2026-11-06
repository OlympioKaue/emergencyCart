namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.GetPending;

public sealed record PendingItemResponse(
    Guid ItemId,
    string ItemName,
    int ExpectedQuantity,
    int FoundQuantity,
    int MissingQuantity,
    int ReplenishedQuantity, 
    int PendingQuantity,
    string? Note);
