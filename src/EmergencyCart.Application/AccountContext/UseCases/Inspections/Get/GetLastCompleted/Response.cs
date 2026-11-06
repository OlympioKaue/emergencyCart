using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetLastCompleted;

public sealed record class Response(Guid InspectionId, DateTime FinishedAt) : IQueryResponse;
