using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.RegisterItem;

public sealed record class Command(Guid InspectionId, Guid ItemId, Guid EmployeeId, int FoundQuantity) : ICommand<Response>;




