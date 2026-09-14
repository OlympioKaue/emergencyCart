using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetById
{
    public sealed record class Response(Guid id, string firstName, string lastName) : IQueryResponse;
}
