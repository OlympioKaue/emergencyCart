using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetAll;

public sealed record class Response(Guid id, string firstName, string lastName, string email, string role, bool active) : IQueryResponse;
