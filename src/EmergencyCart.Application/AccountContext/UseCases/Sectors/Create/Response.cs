using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.Create;

public sealed record class Response(string message, Guid Id) : ICommandResponse;
