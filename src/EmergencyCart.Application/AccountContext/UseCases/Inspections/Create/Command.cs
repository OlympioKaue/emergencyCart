using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create;

public sealed record class Command(Guid EmergencyCartId, Guid EmployeeId) : ICommand<Response>;
