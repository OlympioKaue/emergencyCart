using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetLastCompleted;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.EmergencyCartId)
            .NotEmpty().WithMessage("Emergency cart ID cannot be empty.");
    }
}
