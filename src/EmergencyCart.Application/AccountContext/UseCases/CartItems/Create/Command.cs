using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Create;

public sealed record class Command(Guid emergencyCartId, Guid itemId, int expectedQuantity) : ICommand<Response>;
