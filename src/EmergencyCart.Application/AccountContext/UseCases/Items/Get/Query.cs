using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Items.Get;

public sealed record class Query() : IQuery<IReadOnlyList<Response>>;
