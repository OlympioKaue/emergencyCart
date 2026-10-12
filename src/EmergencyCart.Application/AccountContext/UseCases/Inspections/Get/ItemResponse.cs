using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get;

public sealed record ItemResponse(
    Guid ItemId,
    string ItemName,
    int ExpectedQuantity,
    int? FoundQuantity,
    int? Difference,
    ComparisonStatus Comparison,
    int MissingQuantity);
