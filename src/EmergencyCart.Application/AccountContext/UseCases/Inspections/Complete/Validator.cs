using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Complete;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.InspectionId).NotEmpty().WithMessage("Inspection ID cannot be empty.");
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee ID cannot be empty.");
    }
}