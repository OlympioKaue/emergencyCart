using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Items.Create;

public sealed record class Response(string message) : ICommandResponse;
