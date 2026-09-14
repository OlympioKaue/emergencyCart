using EmergencyCart.Domain.AccountContext.Entities;
using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Create;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(e => e.sectorCode)
            .NotEmpty().WithMessage("SectorCode is not empty")
            .Matches(Sector.Patterntwo).WithMessage("Sector code must follow the format 'SEC-' followed by 8 uppercase hexadecimal characters (e.g., 'CART-3F9A2B1C').");
    }
}
