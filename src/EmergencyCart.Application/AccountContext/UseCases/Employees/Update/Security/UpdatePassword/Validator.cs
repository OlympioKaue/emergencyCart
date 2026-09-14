//using EmergencyCart.Domain.AccountContext.ValueObjects;
//using FluentValidation;

//namespace EmergencyCart.Application.AccountContext.UseCases.Users.Update.Security.UpdatePassword;

//public class Validator : AbstractValidator<Command>
//{
//    public Validator()
//    {
//        RuleFor(y => y.email)
//        .NotEmpty().WithMessage("Email cannot be empty")
//        .Matches(Email.Pattern).WithMessage("Invalid email field. must contain more than 3 characters (ex:name@domain.com)");

//        RuleFor(y => y.passwordAntig)
//          .NotEmpty().WithMessage("Password cannot be empty")
//          .Matches(Password.Pattern).WithMessage("Invalid password field");

//        RuleFor(y => y.newPassword)
//         .NotEmpty().WithMessage("Password cannot be empty")
//         .Matches(Password.Pattern).WithMessage("Invalid password field");
//    }
//}
