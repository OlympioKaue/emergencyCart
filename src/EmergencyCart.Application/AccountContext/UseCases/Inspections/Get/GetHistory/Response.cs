using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public sealed record class Response(
    Guid InspectionId,
    Guid EmergencyCartId,
    string CartCode,
    string SectorName,
    string EmployeeName,
    string Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    IReadOnlyCollection<ItemResponse> Items) : IQueryResponse;
