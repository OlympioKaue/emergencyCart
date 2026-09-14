using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.CartItems.Create;

public sealed record class Response(string message) : ICommandResponse;
