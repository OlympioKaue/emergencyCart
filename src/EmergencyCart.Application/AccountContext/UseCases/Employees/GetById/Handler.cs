using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.GetById;

public sealed class Handler(IEmployeeRepository userRepository) : IQueryHandler<Query, Response>
{
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserAsync(request.id, cancellationToken);

        if (user is null)
            return Result.Failure<Response>(Error.NotFound("404", "User not found"));

        return Result.Success<Response>(new Response(user.Id, user.Name.FirstName, user.Name.LastName));
    }
}
