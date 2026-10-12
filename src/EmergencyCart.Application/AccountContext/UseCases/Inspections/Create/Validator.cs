using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.EmergencyCartId)
            .NotEmpty()
            .WithMessage("Emergency cart ID cannot be empty.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("Employee ID cannot be empty.");
    }
}
