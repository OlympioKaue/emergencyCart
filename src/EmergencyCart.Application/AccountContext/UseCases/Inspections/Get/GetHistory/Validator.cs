using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get.GetHistory;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50.");
        RuleFor(x => x.EmergencyCartId).NotEqual(Guid.Empty).When(x => x.EmergencyCartId.HasValue)
            .WithMessage("Emergency cart ID is invalid.");
    }
}
