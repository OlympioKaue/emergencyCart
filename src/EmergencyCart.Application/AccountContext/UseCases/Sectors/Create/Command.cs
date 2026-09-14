using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.Sectors.Create;

public sealed record class Command(string name) : ICommand<Response>;
