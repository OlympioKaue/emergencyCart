//using EmergencyCart.Application.AccountContext.Repositories.Abstractions;
//using EmergencyCart.Application.SharedContext.Repositories.Abstractions;
//using EmergencyCart.Application.SharedContext.Results;
//using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
//using FluentValidation;

//namespace EmergencyCart.Application.AccountContext.UseCases.Users.Update.Security.UpdatePassword;

//public sealed class Handler(IUserRepository _userRepository, IUnitOfWork _ofWork) : ICommandHandler<Command, Response>
//{
//    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
//    {
//        var validationResult = Validate(request);
//        if (validationResult.IsFailure)
//            return Result.Failure<Response>(validationResult.Error);

//        //VERIFICAR SE EMAIL EXISTE
//        var user = await _userRepository.GetUserEmailAsync(request.email, cancellationToken);
//        if (user is null)
//            return Result.Failure<Response>(Error.NotFound("User.Password.NotFound", "Email not found"));

//        //ATUALIZA O QUE PRECISA
//        var passwordTeste = user.TestePassword(request.passwordAntig);
//        if (passwordTeste is false)
//            return Result.Failure<Response>(Error.BadRequest("User.Password.BadRequest", "Password Invalid"));

//        user.ChangePasswordEmail(request.newPassword);

//        _userRepository.Update(user);
//        await _ofWork.CommitAsync(cancellationToken);

//        return Result.Success(new Response());
//    }

//    private static Result Validate(Command request)
//    {
//        var validation = new Validator();
//        var result = validation.Validate(request);

//        if (result.IsValid)
//            return Result.Success();

//        var errors = result.Errors.Select(x => x.ErrorMessage).ToArray();
//        return Result.Failure<Response>(new ValidationError(errors));
//    }
//}
