using EmergencyCart.Domain.AccountContext.Entities;
using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.Create;

public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(y => y.name)
            .NotEmpty().WithMessage("The name is required.")
            .MaximumLength(60).WithMessage("The name must not exceed 60 characters.")
            .Matches(Sector.Pattern).WithMessage("Sector name must contain only letters and spaces, with at least one space (e.g., 'Clinica Medica').");
    }
}
