using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.DoLogins;

public sealed record class Response(string token, string firstName, string role) : ICommandResponse;
