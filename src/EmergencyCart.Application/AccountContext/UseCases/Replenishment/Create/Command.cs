using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Replenishment.Create;

public sealed record class Command(Guid InspectionId, Guid ItemId, Guid EmployeeId, int Quantity) : ICommand<Response>;
