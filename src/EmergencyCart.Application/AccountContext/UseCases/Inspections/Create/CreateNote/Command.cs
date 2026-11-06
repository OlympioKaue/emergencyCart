using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create.CreateNote;

public sealed record class Command(Guid InspectionId, Guid ItemId, string? Note) : ICommand<Response>;
