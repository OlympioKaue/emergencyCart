using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Inspections.Create.CreateNote;

public sealed record class Response(string message) : ICommandResponse;
