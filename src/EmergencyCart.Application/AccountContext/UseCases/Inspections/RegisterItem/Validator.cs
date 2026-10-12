using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.RegisterItem;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID cannot be empty.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID cannot be empty.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID cannot be empty.");

        RuleFor(x => x.FoundQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Found quantity cannot be negative.")
            .LessThanOrEqualTo(5).WithMessage("Found quantity cannot exceed 5.");
    }
}
