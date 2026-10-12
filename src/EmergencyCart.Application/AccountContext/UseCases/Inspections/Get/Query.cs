using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Get;

public sealed record class Query(Guid InspectionId, bool OnlyMissing) : IQuery<Response>;

