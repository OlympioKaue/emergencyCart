using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll;

public sealed record class Query : IQuery<IReadOnlyList<Response>>;

