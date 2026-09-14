using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Create;

public sealed record class Command(
    string firstName, string lastName, string email, string role, string password) : ICommand<Response>
{
}
