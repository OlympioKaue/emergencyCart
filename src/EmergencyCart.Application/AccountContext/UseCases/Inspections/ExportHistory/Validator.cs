using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.ExportHistory;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Format).IsInEnum().WithMessage("Export format is invalid.");
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty).WithMessage("Employee ID is invalid.");
        RuleFor(x => x.EmergencyCartId).NotEqual(Guid.Empty).When(x => x.EmergencyCartId.HasValue)
            .WithMessage("Emergency cart ID is invalid.");
    }
}