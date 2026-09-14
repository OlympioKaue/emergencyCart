using EmergencyCart.Domain.AccountContext.Enums;
using EmergencyCart.Domain.AccountContext.ValueObjects;
using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Employees.Create;

public class Validator : AbstractValidator<Command>
{
    private static readonly string[] AllowedRoles =
       Enum.GetNames<EmployeeRole>(); // ["Admin", "Nurse", "Pharmacist"]

    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(y => y.firstName)
            .NotEmpty().WithMessage("Fistname cannot be empty")
            .Matches(Name.Pattern).WithMessage("The First Name field must contain only letters and single spaces (no numbers or symbols)");

        RuleFor(y => y.lastName)
            .NotEmpty().WithMessage("Lastname cannot be empty")
            .Matches(Name.Pattern).WithMessage("The Last Name field must contain only letters and single spaces (no numbers or symbols)");

        RuleFor(y => y.email)
            .NotEmpty().WithMessage("Email cannot be empty")
            .Matches(Email.Pattern).WithMessage("Invalid email field. must contain more than 3 characters (ex:name@domain.com)");

        RuleFor(y => y.role)
            .Must(role => AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid role.");

        RuleFor(y => y.password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .Matches(Password.Pattern).WithMessage("Invalid password field");

    }
}
