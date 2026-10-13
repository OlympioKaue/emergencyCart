using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetInProgress;

public sealed record Response(Guid inspectionId) : IQueryResponse;
