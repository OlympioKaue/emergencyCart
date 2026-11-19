namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record ItemResponse(
    string ItemName,
    int ExpectedQuantity,
    int? FoundQuantity,
    int MissingQuantity,
    int ReplenishedQuantity,
    int PendingQuantity,
    string? Note,
    IReadOnlyCollection<ReplenishmentResponse> Replenishments);
