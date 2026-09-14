using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetAll;

public sealed record class Query : IQuery<IReadOnlyList<Response>>
{
}
