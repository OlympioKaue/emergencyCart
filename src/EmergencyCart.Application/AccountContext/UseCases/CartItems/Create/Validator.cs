using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Create;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.expectedQuantity)
            .GreaterThan(0).WithMessage("Expected quantity must be greater than zero.")
            .LessThanOrEqualTo(5).WithMessage("Expected quantity must be less than or equal to 5.");
    }
}