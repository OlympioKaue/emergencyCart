using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.AccountContext.Repositories.Abstractions.Security;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.DoLogins;

public sealed class Handler(IEmployeeRepository employeeRepository, ITokenGenerator tokenGenerator) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetUserEmailAsync(request.email, cancellationToken);
        if (employee is null || !employee.Challenge(request.password))
            return Result.Failure<Response>(Error.Unauthorized("401", "Invalid credentials."));

        if (employee.IsActive is false)
            return Result.Failure<Response>(Error.Forbidden("403", "Employee is inactive."));

        return Result.Success(new Response(tokenGenerator.GenerateToken(employee), employee.Name.FirstName, employee.Roles.ToString()));
    }
}
