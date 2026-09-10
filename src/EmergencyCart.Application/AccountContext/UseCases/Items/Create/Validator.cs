using EmergencyCart.Domain.AccountContext.Entities;
using EmergencyCart.Domain.AccountContext.Enums;
using FluentValidation;

namespace EmergencyCart.Application.AccountContext.UseCases.Items.Create;

public class Validator : AbstractValidator<Command>
{
    private static readonly string[] AllowedCategories =
     Enum.GetNames(typeof(ItemCategory)); // ["Medication", "Material", "Equipment"]

    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Name is product cannot be empty")
            .MinimumLength(5).WithMessage("Name must be at least 5 characters long.")
            .MaximumLength(60).WithMessage("Name must not exceed 60 characters.")
            .Matches(Item.Pattern).WithMessage("Name contains invalid characters or is missing a required space between words.");

        RuleFor(x => x.category)
             .NotEmpty().WithMessage("Category is product cannot be empty")
             .Must(category => AllowedCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Category Invalid. Value Accept: {string.Join(", ", AllowedCategories)}");
    }
}
