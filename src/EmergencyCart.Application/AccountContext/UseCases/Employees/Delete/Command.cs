using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Delete;

public sealed record class Command(Guid id) : ICommand<Response>
{
}
