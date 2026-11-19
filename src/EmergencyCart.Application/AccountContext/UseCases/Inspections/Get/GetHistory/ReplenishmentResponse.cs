namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record ReplenishmentResponse(
    int Quantity,
    DateTime ReplenishedAt,
    string EmployeeName);
