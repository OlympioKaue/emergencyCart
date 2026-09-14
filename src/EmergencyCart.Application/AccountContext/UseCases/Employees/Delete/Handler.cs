using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Delete;

public sealed class Handler(IEmployeeRepository _userRepository, IUnitOfWork _ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserAsync(request.id, cancellationToken);
        if (user is null)
            return Result.Failure<Response>(Error.NotFound("User.Id.NotFound", "User not found"));

        _userRepository.Delete(user);
        await _ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response());
    }
}
