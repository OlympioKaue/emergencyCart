using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.GetPending;

public sealed record class Query() : IQuery<IReadOnlyList<Response>>;
