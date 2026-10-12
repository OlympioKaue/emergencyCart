using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.RegisterItem;

public sealed record class Response(string message) : ICommandResponse;

