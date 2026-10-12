using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Complete;

public sealed record Command(Guid InspectionId, Guid EmployeeId) : ICommand<Response>;

