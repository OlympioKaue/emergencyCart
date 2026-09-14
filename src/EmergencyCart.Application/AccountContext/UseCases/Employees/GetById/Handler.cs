using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetById;

public sealed class Handler(IEmployeeRepository userRepository) : IQueryHandler<Query, Response>
{
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var employeeId = await userRepository.GetUserAsync(request.id, cancellationToken);

        if (employeeId is null)
            return Result.Failure<Response>(Error.NotFound("404", "Expected error: Employee not found"));

        return Result.Success<Response>(new Response(employeeId.Id, employeeId.Name.FirstName, employeeId.Name.LastName));
    }
}
