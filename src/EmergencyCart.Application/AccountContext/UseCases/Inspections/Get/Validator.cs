using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.InspectionId).NotEmpty().WithMessage("Inspection ID cannot be empty.");
    }
}
