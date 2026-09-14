using EmergencyCart.Application.AccountContext.UseCases.CartItems.GetAll;
using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.GetById;

public sealed record class Query(Guid emergencyCartId) : IQuery<IReadOnlyList<Response>>;

