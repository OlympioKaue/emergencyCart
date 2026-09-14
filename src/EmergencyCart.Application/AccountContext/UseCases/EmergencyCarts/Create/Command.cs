using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.EmergencyCarts.Create;

public sealed record class Command(string sectorCode) : ICommand<Response>;

