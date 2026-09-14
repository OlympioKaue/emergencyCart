using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetById;

public sealed record Query(Guid id) : IQuery<Response>;
