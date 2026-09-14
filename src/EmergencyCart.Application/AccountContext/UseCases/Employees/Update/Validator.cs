using EmergencyCart.Domain.AccountContext.ValueObjects;
using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Update;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(y => y.firstName)
           .Matches(Name.Pattern).WithMessage("The First Name field must contain only letters and single spaces (no numbers or symbols)")
           .When(y => !string.IsNullOrEmpty(y.firstName));

        RuleFor(y => y.lastName)
            .Matches(Name.Pattern).WithMessage("The Last Name field must contain only letters and single spaces (no numbers or symbols)")
            .When(y => !string.IsNullOrEmpty(y.lastName));

        RuleFor(y => y.email)
            .Matches(Email.Pattern).WithMessage("Invalid email field. must contain more than 3 characters (ex:name@domain.com)")
            .When(y => !string.IsNullOrEmpty(y.email));
    }
}
