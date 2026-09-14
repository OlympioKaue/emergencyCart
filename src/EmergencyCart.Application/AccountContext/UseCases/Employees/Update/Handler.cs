using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Update;

public sealed class Handler(IEmployeeRepository _userRepository, IUnitOfWork _ofWork) : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var user = await _userRepository.GetUserAsync(request.id, cancellationToken);
        if (user is null)
            return Result.Failure<Response>(Error.NotFound("404", "User not found"));

        user.ChangeNameUpdate(request.firstName, request.lastName);

        user.ChangeEmailUpdate(request.email);

        _userRepository.Update(user);

        await _ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response());
    }

    private static Result Validate(Command request)
    {
        var validation = new Validator();
        var result = validation.Validate(request);

        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}
