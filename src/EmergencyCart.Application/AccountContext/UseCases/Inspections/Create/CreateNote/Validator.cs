using EmergencyCart.Domain.AccountContext.Entities;
using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create.CreateNote;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID cannot be empty.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID cannot be empty.");

        RuleFor(x => x.Note)
            .MaximumLength(InspectionItem.MaxNoteLength)
            .WithMessage("Note cannot exceed 200 characters.");
    }
}