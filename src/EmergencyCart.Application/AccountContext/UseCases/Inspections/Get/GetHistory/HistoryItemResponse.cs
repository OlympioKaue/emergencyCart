namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record class HistoryItemResponse(
    Guid InspectionId,
    Guid EmergencyCartId,
    string CartCode,
    string SectorName,
    string EmployeeName,
    DateTime FinishedAt,
    int TotalItems,
    int MissingItems);
