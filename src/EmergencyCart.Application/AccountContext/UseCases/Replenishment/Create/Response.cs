using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Replenishment.Create;

public sealed record class Response(Guid Id, int PendingQuantity) : ICommandResponse;
