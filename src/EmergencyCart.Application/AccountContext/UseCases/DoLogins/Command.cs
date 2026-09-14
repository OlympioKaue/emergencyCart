using EmergencyCart.Application.SharedContext.UseCases.Abstractions;

namespace EmergencyCart.Application.AccountContext.UseCases.DoLogins;

public sealed record class Command(string email, string password) : ICommand<Response>;