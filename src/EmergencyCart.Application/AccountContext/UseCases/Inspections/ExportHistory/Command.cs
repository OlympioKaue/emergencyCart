using EmergencyCart.Application.SharedContext.UseCases.Abstractions;
using EmergencyCart.Domain.AccountContext.Enums;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.ExportHistory;

public sealed record class Command(Guid? EmergencyCartId, ExportFormat Format, Guid EmployeeId) : ICommand<Response>;
