using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Create;

public sealed record class Response(string message) : ICommandResponse
{
}
