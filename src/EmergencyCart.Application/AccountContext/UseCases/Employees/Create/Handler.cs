using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
using EmergencyCart.Application.SharedContext.Results;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.AccountContext.ValueObjects;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Create;

public sealed class Handler(IEmployeeRepository userRepository, IUnitOfWork ofWork) : ICommandHandler<Command, Response>
{
    private const string Welcome = "Funcionario cadastrado com sucesso!";

    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        var validationResult = Validate(request);
        if (validationResult.IsFailure)
            return Result.Failure<Response>(validationResult.Error);

        var emailExists = await userRepository.VerifyEmailExistsAsync(request.email, cancellationToken);
        if (emailExists)
            return Result.Failure<Response>(Error.Conflict("409", "E-mail already in use"));

        var name = Name.Create(request.firstName, request.lastName);

        var email = Email.Create(request.email);

        var password = Password.Create(request.password);

        var role = Enum.Parse<EmployeeRole>(request.role, ignoreCase: true);

        var user = Employee.Create(name, email, password, role);

        await userRepository.AddUserAsync(user, cancellationToken);
        await ofWork.CommitAsync(cancellationToken);

        return Result.Success(new Response(Welcome));
    }

    private static Result Validate(Command request)
    {
        var validation = new Validator();
        var result = validation.Validate(request);
        if (result.IsValid)
            return Result.Success();

        var errors = result.Errors.Select(e => e.ErrorMessage).ToArray();
        return Result.Failure<Response>(new ValidationError(errors));
    }
}
