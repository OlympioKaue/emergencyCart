using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.GetPending;


public sealed record class Response(
    Guid EmergencyCartId,
    string CartCode,
    string SectorName,
    Guid InspectionId,
    DateTime FinishedAt,
    IReadOnlyCollection<PendingItemResponse> Items) : IQueryResponse;