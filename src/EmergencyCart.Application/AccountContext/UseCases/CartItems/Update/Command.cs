using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Update;

public sealed record class Command(Guid cartItemId, int quantity) : ICommand<Response>
{
}
