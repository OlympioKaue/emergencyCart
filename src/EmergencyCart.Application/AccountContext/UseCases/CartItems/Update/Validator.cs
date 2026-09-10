using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Update;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.quantity)
            .InclusiveBetween(1, 5).WithMessage("Quantity must be between 1 and 5.");
    }
}
