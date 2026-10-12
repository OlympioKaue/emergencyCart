using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create;

public sealed record class Response(Guid id,string message) : ICommandResponse;
