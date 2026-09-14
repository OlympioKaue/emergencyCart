using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetAll;

public sealed class Handler(IEmployeeRepository employeeRepository) : IQueryHandler<Query, IReadOnlyList<Response>>
{
    public async Task<Result<IReadOnlyList<Response>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetEmployeesAsync(cancellationToken);
        var response = employees.Select(i
            => new Response(i.Id, i.Name.FirstName, i.Name.LastName, i.Email.Address, i.Roles.ToString(), i.IsActive)).ToList();

        return Result.Success<IReadOnlyList<Response>>(response);
    }
}
