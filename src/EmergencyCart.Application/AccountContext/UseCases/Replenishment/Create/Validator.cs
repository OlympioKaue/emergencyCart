using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Replenishment.Create;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.InspectionId).NotEmpty().WithMessage("Inspection ID cannot be empty.");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("Item ID cannot be empty.");
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee ID cannot be empty.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(5).WithMessage("Quantity cannot exceed 5.");
    }
}
