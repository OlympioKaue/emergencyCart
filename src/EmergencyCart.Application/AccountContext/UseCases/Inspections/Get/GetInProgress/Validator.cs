using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetInProgress;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.EmergencyCartId).NotEmpty().WithMessage("Emergency cart ID cannot be empty.");
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee ID cannot be empty.");
    }
}