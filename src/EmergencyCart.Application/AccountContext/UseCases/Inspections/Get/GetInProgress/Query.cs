using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetInProgress;

public sealed record Query(Guid EmergencyCartId, Guid EmployeeId) : IQuery<Response>;

